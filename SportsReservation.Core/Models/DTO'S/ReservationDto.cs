using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Models.DTO_S
{
    public class ReservationDto
    {
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CreateDate { get; set; } = DateTime.Now;
        public bool IsPaid { get; set; }
        public string QRCode { get; set; }
    }
}
