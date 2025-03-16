using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Users.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace FPTPlaygroundServer.Features.Users;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class FindUserByName : ControllerBase
{
    public new class Request : PageRequest
    {
        public string? Name { get; set; }
        public SortDir SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("users")]
    [Tags("Users")]
    [SwaggerOperation(Summary = "Get List Of Users By Name",
        Description = """
        This API is for retrieving users by name
        Note: Dùng api này cho bất kỳ chỗ tìm kiếm user nào
        `SortColumn` (optional): name, username
        """
    )]
    [ProducesResponseType(typeof(PageList<FindUserResponse?>), StatusCodes.Status200OK)]
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

        var personalTypes = new[] { ConversationType.Personal, ConversationType.Dating, ConversationType.Friendship };

        var result = await (from u in context.Users
                                .Include (u => u.Specialize)
                            where (u.Name.Contains(request.Name ?? "") || u.UserName.Contains(request.Name ?? "")) && u.Id != user.Id && u.ServerId == user.ServerId && u.Status == UserStatus.Active
                            join f1 in context.Friendships
                                .Where(f => f.UserId == user.Id)
                                on u.Id equals f1.FriendId into friends1
                            from f1 in friends1.DefaultIfEmpty()

                            join f2 in context.Friendships
                                .Where(f => f.FriendId == user.Id)
                                on u.Id equals f2.UserId into friends2
                            from f2 in friends2.DefaultIfEmpty()

                            from convoEntry in context.Conversations
                                .Include(c => c.ConversationMembers)
                                    .ThenInclude(cm => cm.UserMasked)
                                        .ThenInclude(um => um!.User)
                                .Include(c => c.ConversationMembers)
                                    .ThenInclude(cm => cm.UserMasked)
                                        .ThenInclude(um => um!.MaskedAvatar)
                                .Include(c => c.ConversationMembers)
                                    .ThenInclude(cm => cm.User)
                                .Include(c => c.Messages)
                                .Where(c => c.ConversationIndex == null && c.Status == ConversationStatus.Active)
                                .Where(c => 
                                    c.ConversationMembers.Any(cm =>
                                        (cm.UserId.HasValue && cm.UserId == user.Id && cm.User!.ServerId == user.ServerId) ||
                                        (cm.UserMasked != null && cm.UserMasked.UserId == user.Id && cm.UserMasked.User.ServerId == user.ServerId)
                                    ) && //Tồn tại current user không
                                    c.ConversationMembers.Any(cm =>
                                        (cm.UserId.HasValue && cm.UserId == u.Id && cm.User!.ServerId == u.ServerId) ||
                                        (cm.UserMasked != null && cm.UserMasked.UserId == u.Id && cm.UserMasked.User.ServerId == u.ServerId)
                                    ) //Tồn tại tại friend không
                                )
                                .DefaultIfEmpty()

                            select new FindUserResponse
                            {
                                Id = u.Id,
                                UserName = u.UserName,
                                Name = u.Name,
                                AvatarUrl = u.AvatarUrl,
                                Gender = u.Gender,
                                Grade = u.Grade,
                                Status = u.Status,
                                FriendshipStatus = f1 != null ? f1.Status :
                                           f2 != null ? f2.Status :
                                           (FriendshipStatus?)null, // Nếu không có friendship -> null
                                ConversationId = convoEntry != null ? convoEntry.Id : (Guid?)null,
                                ConversationType = convoEntry != null ? convoEntry.Type : (ConversationType?)null,
                                Specialize = u.Specialize,
                            }).Distinct()
                            .OrderByColumn(GetSortProperty(request), request.SortOrder)
                            .ToPagedListAsync(request);
        return Ok(result);
    }

    private static Expression<Func<FindUserResponse, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => c => c.Name,
            "username" => c => c.UserName,
            _ => c => c.Id
        };
    }
}
