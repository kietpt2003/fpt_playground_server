using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Users.Models;

public class UserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public Gender Gender { get; set; }
    public int? Grade { get; set; }
    public UserStatus Status { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public AccountResponse Account { get; set; } = default!;
    public CoinWalletResponse? CoinWallet { get; set; }
    public DiamondWalletResponse? DiamondWallet { get; set; }
    public ServerResponse? Server { get; set; }
}
