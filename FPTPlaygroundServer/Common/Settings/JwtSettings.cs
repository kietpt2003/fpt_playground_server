namespace FPTPlaygroundServer.Common.Settings;

public class JwtSettings
{
    public static readonly string Section = "JWT";

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SigningKey { get; set; } = default!;
}
