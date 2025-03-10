using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Conversations.Mappers;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using FPTPlaygroundServer.Features.Conversations.Models;
using FluentValidation;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GetConversations : ControllerBase
{
    public new class Request
    {
        public ConversationType Type { get; set; }
        public FilterType FilterType { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public enum FilterType
    {
        All, Personal
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0");

            RuleFor(c => c.PageIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("PageIndex must be greater than or equal to 0");

            RuleFor(c => c)
                .Must(c => c.PageSize > c.PageIndex)
                .WithMessage("PageSize must be greater than PageIndex");
        }
    }

    [HttpGet("conversations")]
    [Tags("Conversations")]
    [SwaggerOperation(Summary = "Get Conversations",
        Description = """
        This API is for User retrieving conversations (Admin DO NOT USE!!!)

        - `PageSize`: Lớn hơn 0
        - `PageIndex`: Lớn hơn hoặc bằng 0 và phải nhỏ hơn PageSize
        - `Type`: Bắt buộc phải truyền type vì cần phải ConversationIndex giữa các type nữa
        - `Status`: Status bắt buộc phải là active, nên không cần truyền
        - `SortColumn`: Không cần sortColumn nữa vì theo index hết rồi
        - `FilterType`: Personal/All.
            - Personal là chỉ nhóm của user đó
            - All là cho all nhóm trong cùng server
        """
    )]
    [ProducesResponseType(typeof(List<ConversationResponse?>), StatusCodes.Status200OK)]
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

        query = query.Where(c => c.ConversationIndex != null && c.ConversationIndex >= request.PageIndex * request.PageSize && c.ConversationIndex < (request.PageIndex + 1) * request.PageSize);

        query = query
            .Where(c => c.Type == request.Type)
            .Where(c => c.Status == ConversationStatus.Active)
            .Include(c => c.ConversationMembers)
                .ThenInclude(cm => cm.UserMasked);

        if (request.FilterType == FilterType.Personal)
        {
            query = query.Where(c => c.ConversationMembers.Any(cm =>
                (cm.UserId.HasValue && cm.UserId == user.Id && cm.User!.ServerId == user.ServerId) ||
                (cm.UserMasked != null && cm.UserMasked.UserId == user.Id && cm.UserMasked.User.ServerId == user.ServerId)
            ));
        }
        else
        {
            query = query.Where(c => c.ConversationMembers.Any(cm =>
                (cm.UserId.HasValue && cm.User!.ServerId == user.ServerId) ||
                (cm.UserMasked != null && cm.UserMasked.User.ServerId == user.ServerId)
            ));
        }

        var converstions = await query
            .Select(c => c.ToConversationResponse())
            .ToListAsync();
        if (converstions != null)
        {
            var response = GetConversationSlots(converstions!, request.PageIndex, request.PageSize);

            return Ok(response);
        }
        else
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPS_00)
                .AddReason("server", "Something wrong with server. Try again later")
                .Build();
        }
    }

    private static List<ConversationResponse?> GetConversationSlots(List<ConversationResponse> conversations, int pageIndex, int totalSlots = 9)
    {
        // Tính index bắt đầu và kết thúc của trang
        int startIndex = pageIndex * totalSlots;
        int endIndex = startIndex + totalSlots;

        // Tạo danh sách mặc định có `totalSlots` phần tử là null
        var conversationSlots = new List<ConversationResponse?>(new ConversationResponse?[totalSlots]);

        // Gán Conversation vào đúng vị trí của nó trong trang hiện tại
        foreach (var conversation in conversations)
        {
            if (conversation.ConversationIndex.HasValue)
            {
                int relativeIndex = conversation.ConversationIndex.Value - startIndex; // Tính vị trí trong trang
                if (relativeIndex >= 0 && relativeIndex < totalSlots)
                {
                    conversationSlots[relativeIndex] = conversation;
                }
            }
        }

        return conversationSlots;
    }
}
