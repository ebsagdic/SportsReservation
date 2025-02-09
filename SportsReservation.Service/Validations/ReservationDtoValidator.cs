using FluentValidation;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service.Validations
{
    public class ReservationDtoValidator:AbstractValidator<ReservationDto>
    {
        public ReservationDtoValidator()
        {

            RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Başlangıç zamanı boş olamaz.")
            .GreaterThan(DateTime.Now).WithMessage("Başlangıç zamanı geçmişte olamaz.")
            .Must(BeValidDateTime).WithMessage("Geçerli bir tarih formatı giriniz.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("Bitiş zamanı boş olamaz.")
                .GreaterThan(x => x.StartTime).WithMessage("Bitiş zamanı başlangıç zamanından sonra olmalıdır.")
                .Must((dto, endTime) => (endTime - dto.StartTime).TotalMinutes == 60)
                .WithMessage("Bitiş zamanı, başlangıç zamanından tam 1 saat sonra olmalıdır.");

            RuleSet("AdvancedChecks", () =>
            {
                RuleFor(x => x.StartTime)
                    .GreaterThan(DateTime.Now.AddHours(1))
                    .WithMessage("Rezervasyon en az 1 saat öncesinden yapılmalıdır.");
            });
        }
        private bool BeValidDateTime(DateTime dateTime)
        {
            return dateTime != default(DateTime);
        }
    }
}
