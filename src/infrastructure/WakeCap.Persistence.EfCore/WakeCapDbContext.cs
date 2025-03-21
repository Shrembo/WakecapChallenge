using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WakeCap.Infrastructure;

namespace WakeCap.Persistence.EfCore;

internal sealed class WakeCapDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WakeCapDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public sealed class WakeCapPersistenceModule
{
    public static IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WakeCapDb") ?? string.Empty;
        services.AddDbContext<WakeCapDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }
}