using FPTPlaygroundServer.Data;
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
    }
}
