using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.FriendShips.Models;

public class FriendResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
}
