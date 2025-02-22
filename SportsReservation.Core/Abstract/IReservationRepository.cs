using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract
{
    public interface IReservationRepository
    {
        Task<ReservationInfoWithPaidInfo> GetByGuidIdAsync(Guid id);
        Task<Response<NoDataDto>> DeleteByGuidIdAsync(Guid id);
    }
}
