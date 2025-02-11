using FluentValidation;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service.Validations
{
    public class PaymentDtoValidator : AbstractValidator<PaymentDto>
    {
        public PaymentDtoValidator()
        {
            RuleFor(x=>x.CardNumber).NotEmpty().Matches(@"^\d+$").WithMessage("Kart numarası sadece rakamlardan oluşmalıdır.").Length(16).WithMessage("Kart numarası 16 karakter olmalıdır.");
            
            RuleFor(x=>x.ExpireMonth).NotEmpty().Matches(@"^\d+$").WithMessage("Expire Month sadece rakamlardan oluşmalıdır.").MinimumLength(1).MaximumLength(2).WithMessage("Expire Month 1 ile 2 karakter arasında olmalıdır.");

            RuleFor(x => x.ExpireYear)
            .NotEmpty()
            .Matches(@"^\d+$").WithMessage("Expire Year sadece rakamlardan oluşmalıdır.")
            .Length(4).WithMessage("Expire Year 4 karakter uzunluğunda olmalıdır.")
            .Must(year => int.TryParse(year, out int y) && y >= DateTime.Now.Year)
            .WithMessage($"Expire Year {DateTime.Now.Year} yılından küçük olamaz.");

            RuleFor(x => x.Cvc)
            .NotEmpty()
            .Matches(@"^\d+$").WithMessage("Expire Year sadece rakamlardan oluşmalıdır.")
            .Length(3)
            .WithMessage("CVC 3 karakter olmalıdır");

        }
    }
}
