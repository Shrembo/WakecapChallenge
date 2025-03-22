using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Configurations;

public sealed class WorkerZoneImportLogConfiguration : IEntityTypeConfiguration<WorkerZoneImportLog>
{
    public void Configure(EntityTypeBuilder<WorkerZoneImportLog> builder)
    {
        builder.ToTable("worker_zone_import_log");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
    }
}