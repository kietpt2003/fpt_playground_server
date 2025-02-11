
namespace FPTPlaygroundServer.Data.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? Password { get; set; }
    public Role Role { get; set; }
    public LoginMethod LoginMethod { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<AccountVerify> AccountVerifies { get; set; } = [];
}

public enum Role
{
    Admin, User
}

public enum LoginMethod
{
    Default, Google
}

public enum AccountStatus
{
    Active, Inactive, Pending
}
