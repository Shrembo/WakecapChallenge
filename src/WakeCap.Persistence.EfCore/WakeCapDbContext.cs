using Microsoft.EntityFrameworkCore;


namespace WakeCap.Persistence.EfCore;

public sealed class WakeCapDbContext(DbContextOptions<WakeCapDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WakeCapDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}