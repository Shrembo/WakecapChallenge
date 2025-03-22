using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WakeCap.Application.Contacts.Stores;
using WakeCap.Persistence.EfCore.Stores;

namespace WakeCap.Persistence.EfCore;

public sealed class WakeCapPersistenceModule
{
    public static IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IImportLogsStore, ImportLogsStore>();
        services.AddScoped<IWorkersStore, WorkersStore>();
        services.AddScoped<IWorkerZonesStore, WorkerZonesStore>();
        services.AddScoped<IZonesStore, ZonesStore>();
        var connectionString = configuration.GetConnectionString("WakeCapDb") ?? string.Empty;
        services.AddPooledDbContextFactory<WakeCapDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }
}