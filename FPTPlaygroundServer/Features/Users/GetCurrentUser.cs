using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Common.Filters;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Features.Users.Models;
using FPTPlaygroundServer.Features.Users.Mappers;

namespace FPTPlaygroundServer.Features.Users;

[ApiController]
[JwtValidationFilter]
public class GetCurrentUser : ControllerBase
{
    [HttpGet("users/current")]
    [Tags("Users")]
    [SwaggerOperation(Summary = "Get Current User", Description = "This API is for getting the current authenticated user")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        return Ok(user.ToUserResponse());
    }
}
