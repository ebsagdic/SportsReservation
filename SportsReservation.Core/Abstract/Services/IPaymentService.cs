using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract.Services
{
    public interface IPaymentService
    {
        public Task<Response<PaymentModel>> PaymentIyzico(PaymentDto paymentDto, int amount);
    }
}
