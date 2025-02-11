namespace FPTPlaygroundServer.Features.Auth.Models;

public class LoginGoogleResponse
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Picture { get; set; } = default!;
}
