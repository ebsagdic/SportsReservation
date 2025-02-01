using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Models
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
    }
}
