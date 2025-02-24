using Microsoft.EntityFrameworkCore;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using SportsReservation.Repository.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Repository.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Reservation> _dbSet;


        public ReservationRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Reservation>();
        }
        public async Task<ReservationInfoWithPaidInfo> GetByGuidIdAsync(Guid id)
        {
            var reservations = await _context.Reservations
            .Where(r => r.UserId == id && r.EndTime>DateTime.UtcNow)
            .Select(r => new ReservationInfoWithPaidInfo
            {
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                IsPaid = r.IsPaid
            })
            .AsNoTracking()
            .ToListAsync();


            return reservations.FirstOrDefault();
        }
        public async Task<Response<NoDataDto>> DeleteByGuidIdAsync(Guid id)
        {
            var reservation = await _dbSet.FirstOrDefaultAsync(r => r.UserId == id);

            if (reservation == null)
            {
                return Response<NoDataDto>.Fail(404, new List<string> { "Rezervasyon bulunamadı." });
            }

            _dbSet.Remove(reservation);

            return Response<NoDataDto>.Success(null,200);
        }

    }
}
