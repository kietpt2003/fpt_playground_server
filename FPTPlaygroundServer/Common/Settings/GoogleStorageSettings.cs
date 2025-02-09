namespace FPTPlaygroundServer.Common.Settings;

public class GoogleStorageSettings
{
    public static readonly string Section = "GoogleStorage";

    public string Bucket { get; set; } = default!;
}
