namespace FPTPlaygroundServer.Common.Settings;

public class MailSettings
{
    public static readonly string Section = "SmtpClient";

    public string Mail { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Host { get; set; } = default!;
    public int Port { get; set; }
}
