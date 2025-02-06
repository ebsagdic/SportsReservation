using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models.DTO_S;
using System.Security.Claims;

namespace SportsReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : CustomBaseController
    {
        public readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        [Authorize(Roles ="Yönetici,Personel,Öğrenci")]
        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync(ReservationDto reservationDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            Guid userId = Guid.Parse(userIdClaim.Value); // Eğer GUID ise Guid.Parse() kullan

            reservationDto.UserId = userId; // Kullanıcıdan değil, token'dan al
            return ActionResultInstance(await _reservationService.CreateReservationAsync(reservationDto));
        }
    }
}
