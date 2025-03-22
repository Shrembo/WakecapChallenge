using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace WakeCap.Persistence.EfCore;

public sealed class WakeCapDbContext(DbContextOptions<WakeCapDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WakeCapDbContext).Assembly);
        SetTimeUtcConverter(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }


    // Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported
    private static void SetTimeUtcConverter(ModelBuilder modelBuilder)
    {
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(DateTime)))
            {
                property.SetValueConverter(dateTimeConverter);
            }
        }
    }
}