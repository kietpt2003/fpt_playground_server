namespace FPTPlaygroundServer.Common.Settings;

public class PayOSSettings
{
    public static readonly string Section = "Payment:PayOS";

    public string ClientID { get; set; } = default!;

    public string ApiKey { get; set; } = default!;

    public string ChecksumKey { get; set; } = default!;
}
