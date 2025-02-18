using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Common.Filters;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Users.Models;
using FPTPlaygroundServer.Features.Users.Mappers;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Data;
using Microsoft.EntityFrameworkCore;
using FPTPlaygroundServer.Data.Entities;

namespace FPTPlaygroundServer.Features.Users;

[ApiController]
[JwtValidationFilter]
public class GetCurrentUser : ControllerBase
{
    [HttpGet("users/current")]
    [Tags("Users")]
    [SwaggerOperation(Summary = "Get Current User", Description = "This API is for getting the current authenticated user")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService, [FromServices] AppDbContext context)
    {
        var user = await currentUserService.GetCurrentUser();

        var userLevelPass = await context.UserLevelPasses
            .Include(ulp => ulp.LevelPass)
            .OrderByDescending(ulp => ulp.LevelPass.Level)
            .FirstOrDefaultAsync(ulp => ulp.UserId == user!.Id && ulp.LevelPass.Require >= ulp.Experience && ulp.LevelPass.Status == LevelPassStatus.Active) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("userLevelPass", "userLevelPass empty")
                .Build();

        return Ok(user.ToUserResponse(userLevelPass));
    }
}
