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
            foreach (var server in ServerSeed.Default)
            {
                context.Servers.Add(server);
            }
            await context.SaveChangesAsync();
        }

        if (!await context.LevelPasses.AnyAsync())
        {
            foreach (var levelPass in LevelPassSeed.Default)
            {
                context.LevelPasses.Add(levelPass);
            }
            await context.SaveChangesAsync();
        }

        if (!await context.FaceValues.AnyAsync())
        {
            foreach (var faceValue in FaceValueSeed.Default)
            {
                context.FaceValues.Add(faceValue);
            }
            await context.SaveChangesAsync();
        }
    }
}
