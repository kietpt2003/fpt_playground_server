using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Servers.Mappers;
using FPTPlaygroundServer.Features.Servers.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace FPTPlaygroundServer.Features.Servers;

[ApiController]
public class GetServers : ControllerBase
{
    public new class Request : PageRequest
    {
        public string? Name { get; set; }
        public SortDir? SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    [HttpGet("servers")]
    [Tags("Server")]
    [SwaggerOperation(Summary = "Get Servers",
        Description = """
        This API is for retrieving servers

        `SortColumn` (optional): name
        """
    )]
    [ProducesResponseType(typeof(PageList<ServerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context)
    {
        var query = context.Servers.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
                            .Where(c => c.Name.Contains(request.Name ?? ""))
                            .Select(c => c.ToServerResponse())
                            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<Server, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => c => c.Name,
            _ => c => c.Id
        };
    }
}
