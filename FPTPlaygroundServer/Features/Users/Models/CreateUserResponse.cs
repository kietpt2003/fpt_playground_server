namespace FPTPlaygroundServer.Features.Users.Models;

public class CreateUserResponse
{
    public UserResponse UserResponse { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
