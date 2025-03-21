using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WakeCap.Domain;

namespace WakeCap.Persistence.EfCore.Configurations;

internal sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("zone", t => t.ExcludeFromMigrations());
    }
}
