using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Services.Auth.Models;

public class TokenRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public LoginMethod LoginMethod { get; set; }
    public Role Role { get; set; }
    public AccountStatus Status { get; set; }
}
