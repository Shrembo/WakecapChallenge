using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WakeCap.Infrastructure.CsvParsers;

namespace WakeCap.Infrastructure
{
    public static class WakeCapInfrastructureModule
    {
        public static IServiceCollection Register(IServiceCollection services, IConfiguration _)
        {
            services.AddScoped<ICsvParser, CsvParser>();
            return services;
        }
    }
}