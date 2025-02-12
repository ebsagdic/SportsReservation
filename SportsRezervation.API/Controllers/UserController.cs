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

        [Authorize]
        [HttpGet("GetUserByNameAsync")]
        public async Task<IActionResult> GetUserByNameAsync() 
        {
            var response = await _userService.GetUserByNameAsync(HttpContext.User.Identity.Name);
            return ActionResultInstance(response);
        }
    }
}
