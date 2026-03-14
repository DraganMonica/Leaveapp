using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace LeaveManagementSystem.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            builder.HasData(
               new ApplicationUser
               {
                   Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                   Email = "admin@leaveapp.com",
                   NormalizedUserName = "ADMIN@LEAVEAPP.COM",
                   NormalizedEmail = "ADMIN@LEAVEAPP.COM",
                   UserName = "admin@leaveapp.com",
                   PasswordHash = hasher.HashPassword(null, "Admin123!"),
                   EmailConfirmed = true,
                   FirstName = "Default",
                   LastName = "Admin",
                   DateOfBirth = new DateTime(2003, 1, 1)
               }
           );
        }
    }
}
