using Microsoft.EntityFrameworkCore;

namespace WakeCap.Persistence.EfCore;

internal sealed class WakeCapDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WakeCapDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}