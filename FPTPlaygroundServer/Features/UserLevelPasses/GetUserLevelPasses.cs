using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using FPTPlaygroundServer.Features.UserLevelPasses.Mappers;
using FPTPlaygroundServer.Features.UserLevelPasses.Models;
using FPTPlaygroundServer.Common.Exceptions;

namespace FPTPlaygroundServer.Features.UserLevelPasses;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GetUserLevelPasses : ControllerBase
{
    public new class Request : PageRequest
    {
        public SortDir SortByLevel { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("user-level-passes")]
    [Tags("UserLevelPasses")]
    [SwaggerOperation(
        Summary = "Get List User Level Passes",
        Description = "This API is for get list user level passes."
    )]
    [ProducesResponseType(typeof(PageList<UserLevelPassResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Account have been inactive or not deactivate")
                .Build();
        }

        var query = context.UserLevelPasses
            .Include(ulp => ulp.LevelPass)
            .AsQueryable();

        query = query.OrderByColumn(GetSortProperty("level"), request.SortByLevel);

        var response = await query
            .Where(ulp => ulp.UserId == user!.Id && ulp.LevelPass.Status == LevelPassStatus.Active)
            .Select(ulp => ulp.ToUserLevelPassResponse())
            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<UserLevelPass, object>> GetSortProperty(string SortColumn)
    {
        return SortColumn?.ToLower() switch
        {
            "level" => ulp => ulp.LevelPass.Level,
            _ => ulp => ulp.LevelPass.Level
        };
    }
}
