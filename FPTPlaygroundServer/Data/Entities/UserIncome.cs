namespace FPTPlaygroundServer.Data.Entities;

public class UserIncome
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Value { get; set; }
    public UserIncomeType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}

public enum UserIncomeType
{
    Achievement, Game, DailyCheckpoint, ReferralCode, Deposit, Level
}
