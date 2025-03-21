using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Configurations;

internal sealed class WorkerZoneAssignmentConfiguration : IEntityTypeConfiguration<WorkerZoneAssignment>
{
    public void Configure(EntityTypeBuilder<WorkerZoneAssignment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("worker_zone_assignment", t => t.ExcludeFromMigrations());
    }
}