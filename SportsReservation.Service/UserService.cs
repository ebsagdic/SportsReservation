using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;


namespace SportsReservation.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserService(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }
        public async Task<Response<NoDataDto>> AddUserRole(string newUserRole)
        {
            var roleExists = await _roleManager.RoleExistsAsync(newUserRole);
            var role =  new IdentityRole { Name = newUserRole };
            if (!roleExists)
            {
                await _roleManager.SetRoleNameAsync(role, newUserRole);
            }
            return Response<NoDataDto>.Success(null, 200);
        }

        public async Task<Response<UserDto>> CreateUserAsync(UserForRegisterDto userForRegisterDto)
        {
            var roleExists = await _roleManager.RoleExistsAsync(userForRegisterDto.SelectedRole);
            if (!roleExists)
            {
                var errors = new List<string>();
                errors.Add("Böyle bir Rol tanımlaması bulunmuyor");
                return Response<UserDto>.Fail(400, errors);
            }
            var user = new CustomUser { Email = userForRegisterDto.Email, UserName = userForRegisterDto.UserName, Name = userForRegisterDto.FirstName, Surname = userForRegisterDto.LastName };

            var creatingResult = _userManager.CreateAsync(user,userForRegisterDto.Password);
            if (!creatingResult.IsCompletedSuccessfully)
            {
                var errors = creatingResult.Result.Errors.Select(x =>x.Description).ToList();
                return Response<UserDto>.Fail(400, errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, userForRegisterDto.SelectedRole);
            if (!addToRoleResult.Succeeded)
            {
                var errors = addToRoleResult.Errors.Select(x => x.Description).ToList();
                return Response<UserDto>.Fail(400, errors);
            }

            UserDto userDto = _mapper.Map<UserDto>(user);
            return Response<UserDto>.Success(userDto, 200);
        }

        public Response<List<string>> GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(x => x.Name).ToList();
            return Response<List<string>>.Success(roles, 200);
        }

        public async Task<Response<UserDto>> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var errors = new List<string>();

            if(user == null)
            {
                errors.Add("Kullanıcı bulunamadı");
                return Response<UserDto>.Fail(400,errors);
            }
            UserDto userDto = _mapper.Map<UserDto>(user);
            userDto.UserRole = string.Join(", ", userRoles);
            userDto.Name = user.Name;
            return Response<UserDto>.Success(userDto, 200);
        }
    }
}
