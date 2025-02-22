namespace SportsResevation.UI.Models
{
    public class ReservationInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ReservationInfoListModel
    {
        public List<ReservationInfo> Reservations { get; set; } = new List<ReservationInfo>();
    }
}
