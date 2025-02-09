using FluentValidation;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service.Validations
{
    public class UserForLoginDtoValidator:AbstractValidator<UserForLoginDto>
    {
        public UserForLoginDtoValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"\d").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[^\w\d]").WithMessage("Şifre en az bir özel karakter içermelidir.");
        }
    }
}
