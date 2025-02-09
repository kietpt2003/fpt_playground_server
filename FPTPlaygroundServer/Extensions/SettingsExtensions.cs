using FPTPlaygroundServer.Common.Settings;

namespace FPTPlaygroundServer.Extensions;

public static class SettingsExtensions
{
    public static void AddConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleStorageSettings>(configuration.GetSection(GoogleStorageSettings.Section));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.Section));
        services.Configure<PayOSSettings>(configuration.GetSection(PayOSSettings.Section));
    }
}
