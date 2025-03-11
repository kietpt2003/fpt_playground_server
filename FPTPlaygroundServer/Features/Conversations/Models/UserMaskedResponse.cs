namespace FPTPlaygroundServer.Features.Conversations.Models;

public class UserMaskedResponse
{
    public Guid Id { get; set; }
    public Guid MaskedAvatarId { get; set; }
    public string MaskedTitle { get; set; } = default!;
    public string MaskedName { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
}
