using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Users.Models;

public class FindUserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public Gender Gender { get; set; }
    public int? Grade { get; set; }
    public UserStatus Status { get; set; }
    public FriendshipStatus? FriendshipStatus { get; set; }
    public Guid? ConversationId { get; set; }
    public ConversationType? ConversationType { get; set; }
    public Specialize? Specialize { get; set; }
}
