using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Common.Paginations;
using System.Linq.Expressions;
using FPTPlaygroundServer.Features.DailyCheckpoints.Mappers;

namespace FPTPlaygroundServer.Features.DailyCheckpoints;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class GetWeeklyDailyCheckpoints : ControllerBase
{
    public new class Request : PageRequest
    {
        public SortDir SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    [HttpGet("daily-checkpoint/current-week")]
    [Tags("DailyCheckpoints")]
    [SwaggerOperation(
        Summary = "Get List Weekly Daily Checkpoints",
        Description = """
        This API is for user get list daily checkpoint for current week.

        `SortColumn` (optional): checkinDate
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        DateTime currentTime = DateTime.UtcNow; //Giờ UTC hiện tại
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        DateTime today = DateTime.UtcNow.Date;

        // Tìm ngày đầu tuần hiện tại, bắt đầu từ thứ Hai
        int diff = today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
        DateTime startOfWeek = today.AddDays(-diff); // Bắt đầu từ 7h sáng của ngày đầu tuần. Lưu ý 7h sáng VN tức là 0h UTC. Và lưu xuống DB là 7h +7 => 7 - 7 = 0h UTC

        var query = context.DailyCheckpoints.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
            .Where(dc => dc.CheckInDate >= startOfWeek && dc.CheckInDate < startOfWeek.AddDays(7) && dc.UserId == user.Id)
            .Select(dc => dc.ToDailyCheckpointResponse())
            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<DailyCheckpoint, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "checkindate" => c => c.CheckInDate,
            _ => c => c.Id
        };
    }
}
