using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Servers.Models;

namespace FPTPlaygroundServer.Features.Servers.Mappers;

public static class ServerMapper
{
    public static ServerResponse? ToServerResponse(this Server? server)
    {
        if (server != null)
        {
            return new ServerResponse
            {
                Id = server.Id,
                Name = server.Name,
                State = server.State,
                Status = server.Status,
            };
        }
        return null;
    }
}
