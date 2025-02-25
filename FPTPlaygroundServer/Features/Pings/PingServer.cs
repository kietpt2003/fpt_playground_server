using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Pings;

[ApiController]
public class PingServer : ControllerBase
{
    [HttpGet("ping")]
    [Tags("Ping")]
    [SwaggerOperation(
        Summary = "Ping Server",
        Description = "This API is for ping server"
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Handler()
    {
        return Ok();
    }
}
