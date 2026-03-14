using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagementSystem.Data.Configurations
{
    public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            //asta pt ADMIN
            builder.HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "084de6a2-19f7-44dd-859c-52985c0ae2b8",
                    UserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
                }
            );
        }
    }
}
