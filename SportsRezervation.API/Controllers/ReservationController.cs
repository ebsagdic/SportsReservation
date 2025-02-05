using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models.DTO_S;

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
            return ActionResultInstance(await _reservationService.CreateReservationAsync(reservationDto));
        }
    }
}
