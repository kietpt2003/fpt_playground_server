using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class CreatePersonalConversation : ControllerBase
{
    public new class Request
    {
        public Guid MaskedAvatarId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? ConversationId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.MaskedAvatarId)
                .NotEmpty()
                .WithMessage("MaskedAvatarId cannot be empty");
            RuleFor(c => c.ReceiverId)
                .NotEmpty()
                .WithMessage("ReceiverId cannot be empty");
        }
    }

    [HttpPost("conversation/personal")]
    [Tags("Conversations")]
    [SwaggerOperation(
        Summary = "Create Personal Conversation",
        Description = """
        This API is for user create personal conversation. Note: 
        - Dùng api này để gửi tin nhắn cho người lạ:
            - Ví dụ User A muốn gửi tin nhắn ẩn danh cho User B thì có thể tạo
                1 personal conversation giữa User A với B và nếu họ kết bạn thì thêm userId vào và không cần xóa MaskedAvatarId
            - Tương tự nếu từ Friendship lên Dating thì update lại ConversationType từ Personal => Dating

        - Sender Bắt buộc phải dùng MaskedAvatar để gửi tin nhắn nếu chưa kết bạn
        - ReceiverId là UserId. Tức là trong API get nếu user đó chọn ẩn danh thì sẽ có cả userId lẫn MaskedAvatarId
        - Phòng TH Sender cố tình spam thì Receiver có thể chọn Report hoặc Block
        - Nếu Report thì phải chờ Admin xem xét còn Block thì chặn luôn User đó
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request, AppDbContext context,
        [FromServices] CurrentUserService currentUserService
    )
    {
        var currentTime = DateTime.UtcNow;

        var sender = await currentUserService.GetCurrentUser();
        if (sender!.Status == UserStatus.Inactive || sender.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "Your account have been inactive or not deactivate")
                .Build();
        }

        if (sender.Id == request.ReceiverId)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("user", "You cannot chat with yourself")
                .Build();
        }
        var receiver = await context.Users
            .Include(user => user.Account)
            .FirstOrDefaultAsync(u => u.Id == request.ReceiverId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("receiver", "Receiver not found")
                .Build();
        if (receiver.ServerId != sender.ServerId)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("server", "Cannot chat with other server")
                .Build();
        }
        if (receiver!.Status == UserStatus.Inactive || receiver.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("receiver", "Your receiver account have been inactive or not deactivate")
                .Build();
        }

        var currentFriendship = await context.Friendships.FirstOrDefaultAsync(fs =>
                (fs.UserId == receiver.Id && fs.FriendId == sender.Id) || (fs.FriendId == receiver.Id && fs.UserId == sender.Id));
        if (currentFriendship != null && currentFriendship.Status == FriendshipStatus.Blocked)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("user", "User have been blocked")
                .Build();
        }

        var theirConversation = await context.Conversations
            .Include(c => c.ConversationMembers)
                .ThenInclude(cm => cm.UserMasked)
            .Where(c => c.Type == ConversationType.Personal || c.Type == ConversationType.Dating || c.Type == ConversationType.Friendship)
            .Where(c => c.ConversationMembers.Any(cm => cm.UserId == sender.Id || (cm.UserMasked != null && cm.UserMasked.UserId == sender.Id)) &&
                        c.ConversationMembers.Any(cm => cm.UserId == receiver.Id || (cm.UserMasked != null && cm.UserMasked.UserId == receiver.Id))
            ).FirstOrDefaultAsync();
        if (theirConversation != null) //TH chưa kết bạn hoặc kết bạn rồi nhưng đã chat với nhau rồi => báo lỗi, khỏi tạo
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("conversation", "Conversation exist")
                .Build();
        }

        if (currentFriendship == null) //TH chưa kết bạn => chat với sender là ẩn danh
        {
            Conversation conversation = new()
            {
                Name = "Personal Conversation",
                Type = ConversationType.Personal,
                Status = ConversationStatus.Active,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
            };

            var maskedAvatar = await context.MaskedAvatars.FirstOrDefaultAsync(ma => ma.Id == request.MaskedAvatarId);

            UserMasked maskedSender = new()
            {
                MaskedAvatarId = request.MaskedAvatarId,
                UserId = sender.Id,
                Status = UserMaskedStatus.Active,
                CreatedAt = currentTime,
            };

            ConversationMember currentSender = new()
            {
                Conversation = conversation,
                UserMasked = maskedSender,
                Role = ConversationMemberRole.Owner,
                Status = ConversationMemberStatus.Joined,
                JoinedAt = currentTime,
                UpdatedAt = currentTime,
            };

            ConversationMember currentReceiver = new()
            {
                Conversation = conversation,
                Role = ConversationMemberRole.Member,
                Status = ConversationMemberStatus.Joined,
                JoinedAt = currentTime.AddSeconds(1),
                UpdatedAt = currentTime.AddSeconds(1),
            };

            if (request.ConversationId is not null) //TH sender gửi cho 1 người trong group
            {
                var currentConversation = await context.Conversations
                    .Include(c => c.ConversationMembers)
                        .ThenInclude(cm => cm.UserMasked)
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId) ?? throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("conversation", "Conversation not found")
                    .Build();
                var conversationMembers = currentConversation.ConversationMembers;
                bool isSenderInGroup = conversationMembers.Any(cm => cm.UserId == sender.Id || (cm.UserMasked != null && cm.UserMasked.UserId == sender.Id));
                if (!isSenderInGroup)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("user", "You are not allow to access this group")
                    .Build();
                }
                var receiverInGroup = conversationMembers.FirstOrDefault(cm => cm.UserId == receiver.Id || (cm.UserMasked != null && cm.UserMasked.UserId == receiver.Id)) ?? throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("receiver", "Don't find this receiver in conversation")
                    .Build();
                if (receiverInGroup.UserId is null) //TH receiver trong group là người ẩn danh
                {
                    if (receiverInGroup.UserMasked!.MaskedAvatarId == request.MaskedAvatarId)
                    {
                        throw FPTPlaygroundException.NewBuilder()
                            .WithCode(FPTPlaygroundErrorCode.FPB_02)
                            .AddReason("user", "Sender and Receiver cannot have the same mask")
                            .Build();
                    }

                    UserMasked receiverMask = new()
                    {
                        MaskedAvatarId = receiverInGroup.UserMasked!.MaskedAvatarId,
                        UserId = receiverInGroup.UserMasked!.UserId,
                        Status = UserMaskedStatus.Active,
                        CreatedAt = currentTime.AddSeconds(1),
                    };

                    currentReceiver.UserMasked = receiverMask;
                }
                else //TH receiver trong group không phải là người ẩn danh
                {
                    currentReceiver.User = receiver;
                }
            }
            else //TH sender gửi cho 1 người ngoài group(Tức là dạng tìm ra info của người ta => sender vẫn ẩn danh còn receiver không ẩn). Lưu ý: 2 người vẫn chưa kết bạn
            {
                currentReceiver.User = receiver;
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.ConversationMembers.AddRangeAsync([currentSender, currentReceiver]);

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
                    }
                    else
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                    }
                }
            });
        }
        else //TH 2 người đã kết bạn rồi
        {
            Conversation conversation = new()
            {
                Name = "Friendship Conversation",
                Type = ConversationType.Friendship,
                Status = ConversationStatus.Active,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
            };

            ConversationMember currentSender = new()
            {
                Conversation = conversation,
                UserId = sender.Id,
                Role = ConversationMemberRole.Owner,
                Status = ConversationMemberStatus.Joined,
                JoinedAt = currentTime,
                UpdatedAt = currentTime,
            };

            ConversationMember currentReceiver = new()
            {
                Conversation = conversation,
                UserId = receiver.Id,
                Role = ConversationMemberRole.Member,
                Status = ConversationMemberStatus.Joined,
                JoinedAt = currentTime.AddSeconds(1),
                UpdatedAt = currentTime.AddSeconds(1),
            };

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.ConversationMembers.AddRangeAsync([currentSender, currentReceiver]);

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
                    }
                    else
                    {
                        throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                    }
                }
            });
        }

        return Created();
    }
}
