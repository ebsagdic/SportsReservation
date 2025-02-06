using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models.DTO_S;

namespace SportsReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize(Roles = "Yönetici")]
        [HttpPost("YeniRolEkle")]
        public async Task<IActionResult> AddUserRole(UserRoleDto userRole)
        {
            return ActionResultInstance(await _userService.AddUserRole(userRole));
        }

        [HttpPost("Kaydol")]
        public async Task<IActionResult> Register(UserForRegisterDto userRegisterDto) 
        {
            return ActionResultInstance(await _userService.CreateUserAsync(userRegisterDto));
        }

        [HttpGet("Roller")]
        public IActionResult GetAllRoles()
        {
            return ActionResultInstance( _userService.GetAllRoles());
        }

        [Authorize(Roles = "Yönetici, Öğrenci, Personel")]
        [HttpGet("UserInfo")]
        public async Task<IActionResult> GetUserByNameAsync(GetUserByNameDto getUserByNameDto) 
        {
            return ActionResultInstance(await _userService.GetUserByNameAsync(getUserByNameDto.Name));
        }
    }
}
