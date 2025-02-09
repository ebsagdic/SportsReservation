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
    public class PaymentController : CustomBaseController
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Roles = "Yönetici,Personel,Öğrenci")]
        [HttpPost]
        public async Task<IActionResult> Payment(PaymentDto paymentDto) 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            int amount = roleClaim switch
            {
                "Yönetici" => 500,
                "Personel" => 400,
                "Öğrenci" => 300,
                _ => throw new Exception("Geçersiz kullanıcı rolü!")
            };

            Guid userId = Guid.Parse(userIdClaim.Value);
            paymentDto.UserId = userId;
            return ActionResultInstance(await _paymentService.PaymentIyzico(paymentDto,amount));
        }
    }
}
