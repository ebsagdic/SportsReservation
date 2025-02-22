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
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            Guid userId = Guid.Parse(userIdClaim.Value); // Eğer GUID ise Guid.Parse() kullan

            reservationDto.UserId = userId; // Kullanıcıdan değil, token'dan al
            string userRole = userRoleClaim.Value;
            return ActionResultInstance(await _reservationService.CreateReservationAsync(reservationDto, userRole));
        }

        [Authorize(Roles = "Yönetici,Personel,Öğrenci")]
        [HttpGet("CancelReservation")]
        public async Task<IActionResult> CancelUnpaidReservation()
        {
            var result = await _reservationService.CancelUnpaidReservationsAsync();
            return ActionResultInstance(result);
        }

        [Authorize(Roles = "Yönetici,Personel,Öğrenci")]
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservation();
            return ActionResultInstance(reservations);
        }

        [Authorize(Roles = "Yönetici,Personel,Öğrenci")]
        [HttpGet("Reservasyonum")]
        public async Task<IActionResult> GetMyReservation()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            Guid userId = Guid.Parse(userIdClaim.Value);
            var reservations = await _reservationService.GetMyReservation(userId);
            return ActionResultInstance(reservations);
        }

        [Authorize(Roles = "Yönetici,Personel,Öğrenci")]
        [HttpGet("CancelMyReservation")]
        public async Task<IActionResult> CancelMyReservation()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            Guid userId = Guid.Parse(userIdClaim.Value);
            var reservations = await _reservationService.DeleteMyReservationsAsync(userId);
            return ActionResultInstance(reservations);
        }
    }
}
