using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Features.Servers.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using FPTPlaygroundServer.Features.Conversations.Mappers;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class GetConversations : ControllerBase
{
    public new class Request : PageRequest
    {
        public SortDir SortOrder { get; set; }
        public string? SortColumn { get; set; }
        public ConversationType Type { get; set; }
        public ConversationStatus? Status { get; set; }
        public Guid? ServerId { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("conversations")]
    [Tags("Conversations")]
    [SwaggerOperation(Summary = "Get Conversations",
        Description = """
        This API is for retrieving conversation

        `SortColumn` (optional): conversationIndex, status, type, createdAt
        """
    )]
    [ProducesResponseType(typeof(PageList<ServerResponse>), StatusCodes.Status200OK)]
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

        var query = context.Conversations.AsQueryable();

        query = query
            .OrderByColumn(GetSortProperty(request), request.SortOrder)
            .Where(c => c.Type == request.Type);

        if (request.Status != null)
        {
            query = query
                .Where(c => c.Status == request.Status)
                .Include(c => c.ConversationMembers)
                    .ThenInclude(cm => cm.UserMasked);
        }

        if (user.Account.Role == Role.User)
        {
            query = query.Where(c => c.ConversationMembers.Any(cm =>
                (cm.UserId.HasValue && cm.UserId == user.Id && cm.User!.ServerId == user.ServerId) ||
                (cm.UserMasked != null && cm.UserMasked.UserId == user.Id && cm.UserMasked.User.ServerId == user.ServerId)
            ));
        } else if (user.Account.Role == Role.Admin && request.ServerId != null)
        {
            query = query.Where(c => c.ConversationMembers.Any(cm =>
                (cm.UserId.HasValue && cm.User!.ServerId == request.ServerId) ||
                (cm.UserMasked != null && cm.UserMasked.User.ServerId == request.ServerId)
            ));
        }

        var response = await query
            .Select(c => c.ToConversationResponse())
            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<Conversation, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "conversationIndex" => c => c.ConversationIndex ?? int.MaxValue,
            "type" => c => c.Type,
            "status" => c => c.Status,
            "createdat" => c => c.CreatedAt,
            _ => c => c.Id
        };
    }
}
