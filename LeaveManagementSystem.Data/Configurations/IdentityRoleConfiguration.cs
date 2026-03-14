using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagementSystem.Data.Configurations
{
    public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
               new IdentityRole { Id = "084de6a2-19f7-44dd-859c-52985c0ae2b8", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
               new IdentityRole { Id = "46ed7d4b-3380-4e4b-970f-0fdc0fa6e53b", Name = "Employee", NormalizedName = "EMPLOYEE" },
               new IdentityRole { Id = "65ad5989-4367-46e3-a137-09005de811a4", Name = "Manager", NormalizedName = "MANAGER" }
           );
        }
    }
}
