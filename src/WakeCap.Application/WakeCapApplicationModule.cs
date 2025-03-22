using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WakeCap.Application;

public static class WakeCapApplicationModule
{
    public static IServiceCollection Register(IServiceCollection services, IConfiguration _)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WakeCapApplicationModule).Assembly));
        return services;
    }
}