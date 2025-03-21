using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WakeCap.Persistence.EfCore;

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