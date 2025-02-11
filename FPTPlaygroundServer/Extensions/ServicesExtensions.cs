using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Mail;
using FPTPlaygroundServer.Services.Server;
using FPTPlaygroundServer.Services.VerifyCode;

namespace FPTPlaygroundServer.Extensions;

public static class ServicesExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<MailService>();
        services.AddScoped<VerifyCodeService>();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<CurrentServerService>();
        services.AddScoped<TokenService>();
    }
}
