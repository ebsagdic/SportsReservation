using SportsReservation.Core.Abstract;
using SportsReservation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service
{
    public class ReservationService
    {
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ReservationService(IGenericRepository<Reservation> reservationRepository, IUnitOfWork unitOfWork)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CreateReservationAsync(Reservation reservation)
        {
            var existingReservations = await _reservationRepository.GetAllAsync();
            bool hasExistingReservation = existingReservations.Any(r => r.UserId == reservation.UserId &&
                r.CreateDate >= DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek)));

            if (hasExistingReservation)
            {
                throw new InvalidOperationException("User can only have one reservation per week.");
            }

            // Check for overlapping reservations
            bool isOverlapping = existingReservations.Any(r => r.CreateDate == reservation.CreateDate &&
                ((r.StartTime <= reservation.StartTime && r.EndTime > reservation.StartTime) ||
                 (r.StartTime < reservation.EndTime && r.EndTime >= reservation.EndTime)));

            if (isOverlapping)
            {
                throw new InvalidOperationException("Selected time slot is already reserved.");
            }

            await _reservationRepository.CreateAsync(reservation);
            return true;
            //response türünü booldan reservationdtoya çevirelim dayı
        }
    }
}
