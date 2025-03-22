using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace WakeCap.Persistence.EfCore;

public class WakeCapDbContextFactory : IDesignTimeDbContextFactory<WakeCapDbContext>
{
    public WakeCapDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("WakeCapDb");

        var optionsBuilder = new DbContextOptionsBuilder<WakeCapDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WakeCapDbContext(optionsBuilder.Options);
    }
}