namespace FPTPlaygroundServer.Data.Entities;

public class LeaderBoard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}
