using FluentValidation;
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
                nhiều personal conversation giữa User A với B, tức là không chỉ có 1 mà có thể nhiều
            - Chỉ đến khi họ thành bạn thì không thể tạo thêm nữa

        - Sender Bắt buộc phải dùng MaskedAvatar để gửi tin nhắn
        - ReceiverId có thể là UserId hoặc MaskedAvatarId tùy tình huống, ví dụ người đó trong group chọn ẩn danh thì ReceiverId sẽ là MaskedAvatarId và ngược lại
        - Phòng TH Sender cố tình spam thì Receiver có thể chọn Report hoặc Block
        - Nếu Report thì phải chờ Admin xem xét còn Block thì chặn luôn User đó
        """
    )]
    [ProducesResponseType(typeof(DepositResponse), StatusCodes.Status200OK)]
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

        Conversation conversation = new()
        {
            Name = "Private Conversation",
            Type = ConversationType.Personal,
            Status = ConversationStatus.Active,
            CreatedAt = currentTime,
            UpdatedAt = currentTime,
        };

        Message sysMsg = new()
        {
            Conversation = conversation,
            Type = MessageType.System,
            CreatedAt = currentTime,
        };
        var maskedAvatar = await context.MaskedAvatars.FirstOrDefaultAsync(ma => ma.Id == request.MaskedAvatarId);
        sysMsg.Content = $"{maskedAvatar!.MaskedName} created the chat";

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
            var conversationMember = await context.ConversationMembers
                .Include(cm => cm.UserMasked)
                    .ThenInclude(um => um!.User)
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => (cm.UserId == request.ReceiverId || (cm.UserMasked != null && cm.UserMasked.UserId == request.ReceiverId)) && cm.ConversationId == request.ConversationId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("user", "Receiver not found")
                .Build();

            if (conversationMember.User is null) //Th receiver trong group là người ẩn danh
            {
                if (conversationMember.UserMasked!.User.ServerId != sender.ServerId)
                {
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPB_03)
                        .AddReason("server", "Cannot chat with other server")
                        .Build();
                }
                if (conversationMember.UserMasked!.MaskedAvatarId == request.MaskedAvatarId)
                {
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPB_02)
                        .AddReason("user", "Sender and Receiver cannot have the same mask")
                        .Build();
                }
                if (conversationMember.UserMasked.User.Status == UserStatus.Inactive)
                {
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPB_03)
                        .AddReason("user", "Receiver not exist")
                        .Build();
                }

                UserMasked receiverMask = new()
                {
                    MaskedAvatarId = conversationMember.UserMasked!.MaskedAvatarId,
                    UserId = conversationMember.UserMasked!.UserId,
                    Status = UserMaskedStatus.Active,
                    CreatedAt = currentTime.AddSeconds(1),
                };

                currentReceiver.UserMasked = receiverMask;
            }
            else //TH receiver trong group không phải là người ẩn danh
            {
                if (conversationMember.User.ServerId != sender.ServerId)
                {
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPB_03)
                        .AddReason("server", "Cannot chat with other server")
                        .Build();
                }

                if (conversationMember.User.Status == UserStatus.Inactive)
                {
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPB_03)
                        .AddReason("user", "Receiver not exist")
                        .Build();
                }

                currentReceiver.User = conversationMember.User;
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.ConversationMembers.AddRangeAsync([currentSender, currentReceiver]);
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
                    throw FPTPlaygroundException.NewBuilder()
                        .WithCode(FPTPlaygroundErrorCode.FPS_00)
                        .AddReason("server", "Something wrong with the server")
                        .Build();
                }
            });
        }

        return Created();
    }
}
