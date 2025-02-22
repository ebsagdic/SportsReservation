using AutoMapper;
using SportsReservation.Core.Models;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CustomUser, UserDto>().ReverseMap();
            CreateMap<Reservation, ReservationDto>().ReverseMap();
            CreateMap<Reservation, ReservationInfoDto>().ReverseMap();
        }
    }
}
