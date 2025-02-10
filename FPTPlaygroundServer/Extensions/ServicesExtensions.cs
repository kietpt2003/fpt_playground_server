using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Server;

namespace FPTPlaygroundServer.Extensions;

public static class ServicesExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        //services.AddScoped<MailService>();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<CurrentServerService>();
        services.AddScoped<TokenService>();
    }
}
