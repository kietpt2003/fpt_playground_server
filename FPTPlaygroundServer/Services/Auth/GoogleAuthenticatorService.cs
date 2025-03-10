using OtpNet;

namespace FPTPlaygroundServer.Services.Auth;

public class GoogleAuthenticatorService
{
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GetQrCodeUrl(string userEmail, string secretKey)
    {
        string issuer = "FPTPlayground";
        return $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }
}
