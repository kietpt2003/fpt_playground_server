using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Auth.Models;

public class RegisterAccountRequest
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public Role Role { get; set; }
    public LoginMethod LoginMethod { get; set; }
    public AccountStatus Status { get; set; }
    public ICollection<Device> Devices { get; set; } = [];
}
