using System.ComponentModel.DataAnnotations;

namespace SportsResevation.UI.Models
{
    public class LoginModel
    {
        [Required]
        [MinLength(10)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
