using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Users.Models;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public Role Role { get; set; }
    public LoginMethod LoginMethod { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
