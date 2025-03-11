using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FaceValues.Models;
using FPTPlaygroundServer.Features.FriendShips.Mappers;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace FPTPlaygroundServer.Features.FriendShips;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GetFriendships : ControllerBase
{
    public new class Request : PageRequest
    {
        public SortDir SortOrder { get; set; }
        public string? SortColumn { get; set; }
        public FriendshipStatus Status { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("friendships")]
    [Tags("Friendships")]
    [SwaggerOperation(Summary = "Get List Of Friendships",
        Description = """
        This API is for retrieving friends, blocks, pending request
        Note: Chỉ lấy ra danh sách bạn bè chứ không lấy được first message
        `SortColumn` (optional): createdAt, updatedAt
        """
    )]
    [ProducesResponseType(typeof(PageList<FaceValueResponse?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        var query = context.Friendships.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
            .Include(fs => fs.Friend)
            .Include(fs => fs.User)
            .Where(fs => (request.Status != FriendshipStatus.Blocked || (request.Status == FriendshipStatus.Blocked && fs.UpdatedBy == user.Id)) && //Người bị blocked không thể thấy người blocked mình được
                (fs.UserId == user.Id || fs.FriendId == user.Id) &&
                fs.Status == request.Status
            )
            .Select(fs => fs.ToFriendshipResponse(user.Id))
            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<Friendship, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "createdat" => c => c.CreatedAt,
            "updatedat" => c => c.UpdatedAt,
            _ => c => c.Id
        };
    }
}
