namespace FPTPlaygroundServer.Data.Entities;

public class Server
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public ServerState State { get; set; }
    public ServerStatus Status { get; set; }

    public ICollection<User> Users { get; set; } = [];
}

public enum ServerState
{
    Solitary, Medium, Full
}

public enum ServerStatus
{
    Active, Inactive, Maintenance
}
