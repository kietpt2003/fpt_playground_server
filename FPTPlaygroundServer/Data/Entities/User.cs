namespace FPTPlaygroundServer.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid ServerId { get; set; }
    public string UserName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public Gender Gender { get; set; }
    public int? Grade { get; set; }
    public Guid? SpecializeId { get; set; }
    public UserStatus Status { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account Account { get; set; } = default!;
    public CoinWallet CoinWallet { get; set; } = default!;
    public DiamondWallet DiamondWallet { get; set; } = default!;
    public Specialize? Specialize { get; set; }
    public Server Server { get; set; } = default!;
    public ICollection<ConversationMember> ConversationMembers { get; set; } = [];
    public ICollection<UserIncome> UserIncomes { get; set; } = [];
    public ICollection<UserLevelPass> UserLevelPasses { get; set; } = [];
    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
    public ICollection<UserMasked> UserMaskeds { get; set; } = [];
    public ICollection<DailyCheckpoint> DailyCheckpoints { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<MessageStatus> MessageStatuses { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}

public enum Gender
{
    Male, Female, Bisexual, Other
}

public enum UserStatus
{
    Active, Inactive
}
