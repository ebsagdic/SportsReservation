using AutoMapper;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
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
        private readonly IMapper _mapper;
        public ReservationService(IGenericRepository<Reservation> reservationRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }
        public async Task<Response<ReservationDto>> CreateReservationAsync(ReservationDto reservationDto)
        {
            var existingReservations = await _reservationRepository.GetAllAsync();
            bool hasExistingReservation = existingReservations.Any(r => r.UserId == reservationDto.UserId &&
                r.CreateDate >= DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek)));

            if (hasExistingReservation)
            {
                throw new InvalidOperationException("User can only have one reservation per week.");
            }

            bool isOverlapping = existingReservations.Any(r => r.CreateDate == reservationDto.CreateDate &&
                ((r.StartTime <= reservationDto.StartTime && r.EndTime > reservationDto.StartTime) ||
                 (r.StartTime < reservationDto.EndTime && r.EndTime >= reservationDto.EndTime)));

            if (isOverlapping)
            {
                throw new InvalidOperationException("Selected time slot is already reserved.");
            }
            Reservation reservation = _mapper.Map<Reservation>(reservationDto);
            await _reservationRepository.CreateAsync(reservation);
            await _unitOfWork.CommitAsync();
            return Response<ReservationDto>.Success(reservationDto, 200); 
        }
    }
}
