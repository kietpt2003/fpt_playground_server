using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.FriendShips.Models;

public class FriendshipResponse
{
    public Guid Id { get; set; }
    public FriendResponse Friend { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
