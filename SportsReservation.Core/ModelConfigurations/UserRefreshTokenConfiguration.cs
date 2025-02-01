using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SportsReservation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.ModelConfigurations
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {

        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.Property(x => x.UserId).HasMaxLength(450);
            builder.Property(x => x.Code).HasMaxLength(200);
        }
    }
}
