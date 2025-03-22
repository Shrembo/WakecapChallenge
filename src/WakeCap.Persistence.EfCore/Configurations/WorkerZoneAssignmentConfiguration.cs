using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Configurations;

internal sealed class WorkerZoneAssignmentConfiguration : IEntityTypeConfiguration<WorkerZoneAssignment>
{
    public void Configure(EntityTypeBuilder<WorkerZoneAssignment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkerId).HasColumnName("worker_id");
        builder.Property(x => x.ZoneId).HasColumnName("zone_id");
        builder.Property(x => x.EffectiveDate).HasColumnName("effective_date");
        builder.ToTable("worker_zone_assignment", t => t.ExcludeFromMigrations());
    }
}