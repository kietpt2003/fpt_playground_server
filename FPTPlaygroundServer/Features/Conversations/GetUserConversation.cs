using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Conversations.Models;
using FPTPlaygroundServer.Features.FriendShips.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GetUserConversation : ControllerBase
{
    public new class Request : PageRequest
    {
        public FilterType FilterType { get; set; }
    }

    public enum FilterType
    {
        Friends, Strangers
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("conversations/user")]
    [Tags("Conversations")]
    [SwaggerOperation(Summary = "Get User Conversations",
        Description = """
        This API is for User retrieving conversations with Type Personal and Dating (Admin DO NOT USE!!!)

        - `PageSize`: Lớn hơn 0
        - `Page`: Lớn hơn hoặc bằng 0 và phải nhỏ hơn PageSize
        - `Status`: Status không cần truyền vì đoạn chat bị block thì vẫn xem được
        - `FilterType`: Friends/Strangers.
            - Friends là bao gồm bạn bè và ny
            - Strangers là chỉ những người chưa kết bạn
        - Friends có thể xem được những người đã block chỉ là không chat được thôi (đối với người block sẽ thấy được ava của người bị block và ngược lại thì không)
        - Khi unblock sẽ hủy kết bạn nên muốn xem lại đoạn chat vs người đó thì phải vào mục Strangers để xem
        - Stranger có thể xem được những người đã block chỉ là không chat được thôi (đối với người block sẽ thấy được ava của người bị block hoặc ava masked, và ngược lại thì không)
        """
    )]
    [ProducesResponseType(typeof(PageList<UserConversationResponse?>), StatusCodes.Status200OK)]
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
            .Where(c => c.ConversationMembers.Any(cm =>
                (cm.UserId.HasValue && cm.UserId == user.Id && cm.User!.ServerId == user.ServerId) ||
                (cm.UserMasked != null && cm.UserMasked.UserId == user.Id && cm.UserMasked.User.ServerId == user.ServerId)
            ));

        if (request.FilterType == FilterType.Friends)
        {
            query = query.Where(c => c.Type == ConversationType.Dating || c.Type == ConversationType.Friendship);
        }
        else
        {
            query = query.Where(c => c.Type == ConversationType.Personal);
        }

        var pagedConverstions = await query
            .ToPagedListAsync(request);
        var conversations = pagedConverstions.Items;

        List<UserConversationResponse> listResponse = [];
        foreach (var conversation in conversations)
        {
            var firstMessage = conversation.Messages
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            var friendCM = conversation.ConversationMembers.FirstOrDefault(cm => (cm.UserId.HasValue && cm.UserId != user.Id) ||
                (cm.UserMasked != null && cm.UserMasked.UserId != user.Id)
            );

            var friendship = await context.Friendships
                .Include(fs => fs.User)
                    .ThenInclude(u => u.Account)
                .Include(fs => fs.Friend)
                    .ThenInclude(u => u.Account)
                .FirstOrDefaultAsync(fs => (friendCM!.UserId.HasValue && fs.UserId == user.Id && fs.FriendId == friendCM.UserId) || 
                    (friendCM!.UserId.HasValue && fs.UserId == friendCM.UserId && fs.FriendId == user.Id) ||
                    (friendCM!.UserMaskedId.HasValue && fs.UserId == user.Id && fs.FriendId == friendCM.UserMasked!.UserId) ||
                    (friendCM!.UserMaskedId.HasValue && fs.UserId == friendCM.UserMasked!.UserId && fs.FriendId == user.Id)
                );

            UserConversationResponse response = new()
            {
                Id = conversation.Id,
                Type = conversation.Type,
                IsBlocked = (friendship != null && friendship.Status == FriendshipStatus.Blocked),
                IsBlockedBy = (friendship != null && friendship.Status == FriendshipStatus.Blocked) ? friendship.UpdatedBy : null,
                Status = conversation.Status,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
            };

            if (friendCM!.UserId.HasValue)
            {
                FriendResponse friendResponse = new()
                {
                    Id = (Guid)friendCM.UserId,
                    UserName = friendCM.User!.UserName,
                    Name = friendCM.User!.Name,
                    AvatarUrl = friendCM.User!.AvatarUrl,
                    Status = friendCM.User!.Status,
                };
                response.Friend = friendResponse;
            }
            else if (!friendCM!.UserId.HasValue && friendCM.UserMaskedId.HasValue)
            {
                UserMaskedResponse userMaskedResponse = new()
                {
                    Id = (Guid)friendCM.UserMaskedId!,
                    MaskedAvatarId = friendCM.UserMasked!.MaskedAvatarId,
                    MaskedTitle = friendCM.UserMasked!.MaskedAvatar.MaskedTitle,
                    MaskedName = friendCM.UserMasked!.MaskedAvatar.MaskedName,
                    AvatarUrl = friendCM.UserMasked!.MaskedAvatar.AvatarUrl,
                };
                response.UserMasked = userMaskedResponse;
            }

            if (firstMessage != null)
            {
                var firstMsg = await context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.UserMasked)
                        .ThenInclude(um => um.MaskedAvatar)
                    .Include(m => m.MessageStatuses)
                    .FirstOrDefaultAsync(m => m.Id == firstMessage.Id);

                var messageStatus = firstMsg!.MessageStatuses.FirstOrDefault();
                bool isRead = messageStatus != null && messageStatus.ReadAt >= DateTime.UtcNow;

                FirstMessageResponse firstMessageResponse = new()
                {
                    Id = firstMsg!.Id,
                    ConversationId = firstMsg!.ConversationId,
                    Content = firstMsg!.Content,
                    Type = firstMsg!.Type,
                    IsRead = isRead,
                    CreatedAt = firstMsg!.CreatedAt,
                };

                if (firstMsg!.SenderId.HasValue)
                {
                    FriendResponse senderResponse = new()
                    {
                        Id = (Guid)firstMsg!.SenderId!,
                        UserName = friendCM.User!.UserName,
                        Name = friendCM.User!.Name,
                        AvatarUrl = friendCM.User!.AvatarUrl,
                        Status = friendCM.User!.Status,
                    };
                    firstMessageResponse.Sender = senderResponse;
                }
                else if (!firstMsg!.SenderId.HasValue && firstMsg.UserMaskedId.HasValue)
                {
                    UserMaskedResponse userMaskedResponse = new()
                    {
                        Id = (Guid)friendCM.UserMaskedId!,
                        MaskedAvatarId = friendCM.UserMasked!.MaskedAvatarId,
                        MaskedTitle = friendCM.UserMasked!.MaskedAvatar.MaskedTitle,
                        MaskedName = friendCM.UserMasked!.MaskedAvatar.MaskedName,
                        AvatarUrl = friendCM.UserMasked!.MaskedAvatar.AvatarUrl,
                    };
                    firstMessageResponse.UserMasked = userMaskedResponse;
                }

                response.FirstMessage = firstMessageResponse;
            }

            listResponse.Add(response);
        }

        var newList = listResponse
            .OrderByDescending(uc => uc.FirstMessage != null) // FirstMessage null xuống cuối
            .ThenByDescending(uc => uc.FirstMessage != null ? uc.FirstMessage.CreatedAt : DateTime.MinValue) // Sắp xếp theo CreatedAt
            .ThenByDescending(uc => uc.CreatedAt) // Nếu null, sắp xếp theo ngày tạo chat giảm dần
            .ToList();

        var userConversationResponseList = new PageList<UserConversationResponse>(
            newList,
            request.Page ?? 1,
            request.PageSize ?? 10,
            listResponse.Count
        );
        return Ok(userConversationResponseList);
    }
}
