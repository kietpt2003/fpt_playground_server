using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Seeds;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Extensions;

public static class ApplyMigrationsExtensions
{
    public static async void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        if (!await context.Servers.AnyAsync())
        {
            foreach (var brand in ServerSeed.Default)
            {
                context.Servers.Add(brand);
            }
            await context.SaveChangesAsync();
        }
    }
}
