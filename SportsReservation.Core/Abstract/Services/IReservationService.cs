using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract.Services
{
    public interface IReservationService
    {
        public Task<Response<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto, string userRole);
        public Task<Response<List<ReservationInfoDto>>> GetAllReservation();
        public Task<Response<ReservationInfoWithPaidInfo>> GetMyReservation(Guid userId);
        public Task<Response<NoDataDto>> CancelUnpaidReservationsAsync();
        public Task<Response<NoDataDto>> DeleteMyReservationsAsync(Guid userId);

    }
}
