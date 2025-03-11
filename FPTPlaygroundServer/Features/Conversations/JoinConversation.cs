using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Payments.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
public class JoinConversation : ControllerBase
{
    public new class Request
    {
        public Guid? MaskedAvatarId { get; set; }
    }

    [HttpPost("conversation/join-group/{ConversationId}")]
    [Tags("Conversations")]
    [SwaggerOperation(
        Summary = "Join Group Conversation",
        Description = """
        This API is for user join group conversation.
        - User có thể join lại group sau 30 phút nếu out tự nguyện
        - User có thể join lại group sau 1 ngày nếu bị kick
        - Nếu User bị đuổi/tự out quá 3 lần thì không thể join lại nữa
        """
    )]
    [ProducesResponseType(typeof(DepositResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request,
        [FromRoute] Guid ConversationId, AppDbContext context,
        [FromServices] CurrentUserService currentUserService
        )
    {
        var currentTime = DateTime.UtcNow;

        var user = await currentUserService.GetCurrentUser();
        if (user!.Status == UserStatus.Inactive || user.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }
        var conversation = await context.Conversations.FirstOrDefaultAsync(c => c.Id == ConversationId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("conversation", "Conversation not found")
                .Build();

        if (conversation.Status == ConversationStatus.Inactive)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("conversation", "Conversation has been inactive")
                .Build();
        }

        if (conversation.Type == ConversationType.Personal || conversation.Type == ConversationType.Dating || conversation.Type == ConversationType.Friendship)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_01)
                .AddReason("conversation", "You cannot join this conversation")
                .Build();
        }

        bool isOtherServer = await context.ConversationMembers
            .Include(cm => cm.User)
            .Include(cm => cm.UserMasked)
                .ThenInclude(um => um!.User)
            .AnyAsync(cm => cm.ConversationId == ConversationId && ((cm.User != null && cm.User.ServerId != user.ServerId) || (cm.UserMasked != null && cm.UserMasked.User.ServerId != user.ServerId)));
        if (isOtherServer)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_01)
                .AddReason("server", "You cannot join this conversation")
                .Build();
        }

        var oldMember = await context.ConversationMembers
                .Include(cm => cm.Conversation)
                .Include(cm => cm.UserMasked)
                .OrderByDescending(cm => cm.JoinedAt)
                .FirstOrDefaultAsync(cm =>
                (cm.UserId == user.Id || (cm.UserMasked != null && cm.UserMasked.UserId == user.Id)) && cm.ConversationId == ConversationId);

        if (oldMember is not null)
        {
            if (oldMember.Status == ConversationMemberStatus.Joined)
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("user", "User currently join in")
                .Build();
            } else if (oldMember.Status == ConversationMemberStatus.Outed)
            {
                var rejoinTime = oldMember.UpdatedAt.AddMinutes(30);
                var remainingTime = rejoinTime - currentTime;

                if (remainingTime.TotalSeconds > 0)
                {
                    int remainingMinutes = (int)remainingTime.TotalMinutes;
                    int remainingSeconds = remainingTime.Seconds;

                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("user", $"Please wait {remainingMinutes} minutes {remainingSeconds} seconds to join again")
                    .Build();
                }
            } else if (oldMember.Status == ConversationMemberStatus.Kicked)
            {
                var rejoinTime = oldMember.UpdatedAt.AddDays(1);
                var remainingTime = rejoinTime - currentTime;

                if (remainingTime.TotalSeconds > 0)
                {
                    int remainingHours = (int)remainingTime.TotalHours;
                    int remainingMinutes = remainingTime.Minutes;
                    int remainingSeconds = remainingTime.Seconds;

                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("user", $"Please wait {remainingHours} hours {remainingMinutes} minutes {remainingSeconds} seconds to join again")
                    .Build();
                }
            }

            var totalKickedAndOuted = await context.ConversationMembers
                .Include(cm => cm.Conversation)
                .Include(cm => cm.UserMasked)
                .Where(cm =>
                (cm.UserId == user.Id || (cm.UserMasked != null && cm.UserMasked.UserId == user.Id)) && cm.ConversationId == ConversationId &&
                cm.Status != ConversationMemberStatus.Joined
                )
            .ToListAsync();

            var totalKickeds = totalKickedAndOuted.Where(t => t.Status == ConversationMemberStatus.Kicked).ToList();
            if (totalKickeds.Count >= 3)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("user", $"You cannot join this conversation anymore")
                    .Build();
            }

            var totalOuteds = totalKickedAndOuted.Where(t => t.Status == ConversationMemberStatus.Outed).ToList();
            if (totalOuteds.Count >= 3)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("user", $"You cannot join this conversation anymore")
                    .Build();
            }
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                ConversationMember conversationMember = new()
                {
                    ConversationId = ConversationId,
                    UserId = request?.MaskedAvatarId == null ? user.Id : null,
                    Role = ConversationMemberRole.Member,
                    Status = ConversationMemberStatus.Joined,
                    JoinedAt = currentTime,
                    UpdatedAt = currentTime,
                };
                Message sysMsg = new()
                {
                    ConversationId = ConversationId,
                    Type = MessageType.System,
                    CreatedAt = currentTime,
                };

                if (request?.MaskedAvatarId != null)
                {
                    var isInUsed = await context.ConversationMembers
                        .Include(cm => cm.UserMasked)
                        .AnyAsync(cm => cm.ConversationId == ConversationId && cm.UserMasked != null && cm.UserMasked.MaskedAvatarId == request.MaskedAvatarId && cm.Status == ConversationMemberStatus.Joined);
                    if (isInUsed)
                    {
                        throw FPTPlaygroundException.NewBuilder()
                            .WithCode(FPTPlaygroundErrorCode.FPB_02)
                            .AddReason("maskedAvatar", $"This mask has been used by another User.")
                            .Build();
                    }
                    UserMasked userMasked = new()
                    {
                        MaskedAvatarId = (Guid)request.MaskedAvatarId,
                        UserId = user.Id,
                        Status = UserMaskedStatus.Active,
                        CreatedAt = currentTime,
                    };

                    conversationMember.UserMasked = userMasked;

                    var maskedAvatar = await context.MaskedAvatars.FirstOrDefaultAsync(ma => ma.Id == request.MaskedAvatarId);
                    sysMsg.Content = $"{maskedAvatar!.MaskedName} joined the group";
                } else
                {
                    sysMsg.Content = $"{user.UserName} joined the group";
                }

                await context.ConversationMembers.AddAsync(conversationMember);
                await context.Messages.AddAsync(sysMsg);

                // Lưu tất cả vào database
                await context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();

                if (ex is FPTPlaygroundException fptPlagroundException)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(fptPlagroundException.ErrorCode)
                    .AddReasons(fptPlagroundException.GetReasons().Select(reason => new FPTPlaygroundException.Reason(reason.Title, reason.ReasonMessage)))
                    .Build();
                } else
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPS_00)
                    .AddReason("server", "Something wrong with the server")
                    .Build();
                }
            }
        });

        return Created();
    }
}
