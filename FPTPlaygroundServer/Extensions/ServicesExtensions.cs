using FPTPlaygroundServer.Data.Seeds;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Mail;
using FPTPlaygroundServer.Services.Notifications;
using FPTPlaygroundServer.Services.Payment;
using FPTPlaygroundServer.Services.Redis;
using FPTPlaygroundServer.Services.Server;
using FPTPlaygroundServer.Services.Storage;
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
        services.AddScoped<PayOSPaymentSerivce>();
        services.AddScoped<FCMNotificationService>();
        services.AddScoped<RedisService>();
        services.AddScoped<GoogleStorageService>();
        services.AddScoped<GoogleAuthenticatorService>();
        services.AddSingleton<MaskedAvatarSeed>();
    }
}
