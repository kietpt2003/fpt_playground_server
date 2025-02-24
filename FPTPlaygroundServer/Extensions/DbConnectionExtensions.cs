using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Extensions;

public static class DbConnectionExtensions
{
    public static void AddDbContextConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostGreSQL"),
                o =>
                {
                    o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
        });
    }
}
