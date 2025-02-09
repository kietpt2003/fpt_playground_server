namespace FPTPlaygroundServer.Data.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Token { get; set; } = default!;

    public Account Account { get; set; } = default!;
}
