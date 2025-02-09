using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Models
{
    public class Reservation: GeneralModel
    {
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? QRCode { get; set; }

    }
}
