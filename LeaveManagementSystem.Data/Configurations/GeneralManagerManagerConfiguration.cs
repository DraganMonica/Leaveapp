using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagementSystem.Data.Configurations;

public class GeneralManagerManagerConfiguration : IEntityTypeConfiguration<GeneralManagerManager>
{
    public void Configure(EntityTypeBuilder<GeneralManagerManager> builder)
    {
        builder.HasKey(x => new { x.GeneralManagerId, x.ManagerId });

        builder.HasOne(x => x.GeneralManager)
            .WithMany()
            .HasForeignKey(x => x.GeneralManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
