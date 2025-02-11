using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Data.Seeds;

public static class ServerSeed
{
    public readonly static List<Server> Default =
    [
        new Server { Id = Guid.Parse("df3117d4-a6d4-4316-b946-6ae10e5d2b09"), Name = "Xavalo", State = ServerState.Solitary, Status = ServerStatus.Active },
        new Server { Id = Guid.Parse("155fdf09-cb0a-4830-b2f9-2f53be07d0ba"), Name = "Hola", State = ServerState.Solitary, Status = ServerStatus.Active },
        new Server { Id = Guid.Parse("02f3ebac-aea4-45c0-81da-a24a401cc363"), Name = "Hovilo", State = ServerState.Solitary, Status = ServerStatus.Active },
        new Server { Id = Guid.Parse("2e674178-9089-4b4c-a3ed-603642a4f929"), Name = "Quy Nhơn", State = ServerState.Solitary, Status = ServerStatus.Active },
        new Server { Id = Guid.Parse("a2ae376d-f05e-492c-9c5b-2d6a5983e7e5"), Name = "Fuda", State = ServerState.Solitary, Status = ServerStatus.Active },
     ];
}
