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
    public Specialize? Specialize { get; set; }
    public Server Server { get; set; } = default!;
}

public enum Gender
{
    Male, Female, Bisexual, Other
}

public enum UserStatus
{
    Active, Inactive
}
