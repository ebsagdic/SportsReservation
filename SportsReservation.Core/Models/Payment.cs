using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Models
{
    public class PaymentModel: GeneralModel
    {
        public int ReservationId { get; set; }
        public int Amount { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

    }
}
