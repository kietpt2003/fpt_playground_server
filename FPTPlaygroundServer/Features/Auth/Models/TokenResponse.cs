namespace FPTPlaygroundServer.Features.Auth.Models;

public record TokenResponse
{
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
