using FluentValidation;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Service.Validations
{
    public class UserForRegisterDtoValidator : AbstractValidator<UserForRegisterDto>
    {
        public UserForRegisterDtoValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
                .MaximumLength(70).WithMessage("Kullanıcı adı en fazla 70 karakter olabilir.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.") 
                .Matches(@"\d").WithMessage("Şifre en az bir rakam içermelidir.") 
                .Matches(@"[^\w\d]").WithMessage("Şifre en az bir özel karakter içermelidir."); 


            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad alanı boş olamaz.")
                .MaximumLength(70).WithMessage("Ad en fazla 70 karakter olabilir.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad alanı boş olamaz.")
                .MaximumLength(70).WithMessage("Soyad en fazla 70 karakter olabilir.");

            RuleFor(x => x.SelectedRole)
                .NotEmpty().WithMessage("Rol boş olamaz.")
                .MaximumLength(20).WithMessage("Rol en fazla 20 karakter olabilir.");
        }
    }
}
