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
        public async Task<Response<UserRoleDto>> AddUserRole(UserRoleDto newUserRole)
        {
            var roleExists = await _roleManager.RoleExistsAsync(newUserRole.userRoles);
            var role = new IdentityRole { Name = newUserRole.userRoles };
            if (roleExists)
            {
                var errors = new List<string>();
                errors.Add("Rol zaten mevcut");
                return Response<UserRoleDto>.Fail(400,errors);
            }
            if (!roleExists)
            {
                await _roleManager.CreateAsync(role);
            }
            return Response<UserRoleDto>.Success(null, 200);
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
            var user = new CustomUser { Email = userForRegisterDto.Email,NormalizedEmail = userForRegisterDto.Email.ToUpper(), UserName = userForRegisterDto.UserName, Name = userForRegisterDto.FirstName, Surname = userForRegisterDto.LastName,UserType = userForRegisterDto.SelectedRole };

            var creatingResult = await _userManager.CreateAsync(user, userForRegisterDto.Password);
            if (!creatingResult.Succeeded)
            {
                var errors = creatingResult.Errors.Select(x => x.Description).ToList();
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
            if (roles.Count == 0)
            {
                return Response<List<string>>.Success(null,204);
            }
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
