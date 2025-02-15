using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Users.Models;

public class ServerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public ServerState State { get; set; }
    public ServerStatus Status { get; set; }
}
