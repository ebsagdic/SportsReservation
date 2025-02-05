using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models.DTO_S;

namespace SportsReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthanticationController : CustomBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthanticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var response = await _authenticationService.LoginAsync(userForLoginDto);
            return ActionResultInstance(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var response = await _authenticationService.CreateTokenByRefreshToken(refreshTokenDto.RefreshToken);
            return ActionResultInstance(response);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authenticationService.RevokeRefreshToken(refreshTokenDto.RefreshToken);
            return ActionResultInstance(result);
        }

    }
}
