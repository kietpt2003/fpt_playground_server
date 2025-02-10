namespace FPTPlaygroundServer.Data.Entities;

public class MaskedAvatar
{
    public Guid Id { get; set; }
    public string MaskedName { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
    public MaskedAvatarStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserMasked> UserMaskeds { get; set; } = [];
}

public enum MaskedAvatarStatus
{
    Avtive, Inactive
}
