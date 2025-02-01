using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsReservation.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.ModelConfigurations
{
    public class CustomUserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired(); 

            builder.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired(); 

            builder.Property(x => x.UserType)
                .HasMaxLength(100)
                .IsRequired(); 

            builder.Property(x => x.Email)
                .HasConversion(
                    v => v.ToLower(),
                    v => v);
        }
    }
}
