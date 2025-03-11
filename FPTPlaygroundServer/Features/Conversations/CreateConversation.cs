using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Payments.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Features.Conversations;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class CreateConversation : ControllerBase
{
    public new class Request
    {
        public string Name { get; set; } = default!;
        public ConversationType Type { get; set; }
        public int ConversationIndex { get; set; }
        public Guid? MaskedAvatarId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("Name cannot be empty");
            RuleFor(c => c.ConversationIndex)
                .NotNull()
                .WithMessage("ConversationIndex cannot be null")
                .GreaterThanOrEqualTo(0)
                .WithMessage("ConversationIndex must be greater than or equal to 0");
            RuleFor(c => c.Type)
                .Must(type => type != ConversationType.Personal && type != ConversationType.Dating && type != ConversationType.Friendship)
                .WithMessage("Type cannot be Personal or Dating or Friendship");
        }
    }

    [HttpPost("conversation/group")]
    [Tags("Conversations")]
    [SwaggerOperation(
        Summary = "Create Group Conversation",
        Description = """
        This API is for user create group conversation. Note: 
        - Number of person in group can be 1:
            - Giả sử người dùng ấn vào để xí chỗ trước chẳng hạn, nên group mới tạo chắc chắn chỉ có 1 người

        - ConversationIndex:
            - Trong cùng 1 Type **ConversationIndex** không được trùng nhau
        - 1 User chỉ được tạo 3 Conversation khác ConversationType.Personal còn nếu là ConversationType.Personal thì muốn bao nhiêu cũng dc miễn là có bnhiu đó bạn tương ứng
        - User không thể sở hữu quá 1 Conversation cùng ConversationType
        - ConversationType: **StudyGroup**, **DatingGroup**, **CuriousGroup**
        """
    )]
    [ProducesResponseType(typeof(DepositResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request, AppDbContext context,
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

        if (request.Type != ConversationType.Personal)
        {
            bool isExist = await context.Conversations.AnyAsync(c => 
                c.Type != ConversationType.Personal && 
                c.Type == request.Type && 
                c.Type != ConversationType.Dating && 
                c.ConversationIndex == request.ConversationIndex && 
                c.ConversationIndex != null &&
                c.Status == ConversationStatus.Active
            );
            if (isExist)
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("conversation", "This position has been occupied")
                .Build();
            }

            var conversationMembers = await context.ConversationMembers
                .Include(cm => cm.Conversation)
                .Include(cm => cm.UserMasked)
                .Where(cm =>
                (cm.UserId == user.Id || (cm.UserMasked != null && cm.UserMasked.UserId == user.Id)) &&
                cm.Role == ConversationMemberRole.Owner &&
                cm.Conversation.Type != ConversationType.Personal &&
                cm.Conversation.Type != ConversationType.Dating &&
                cm.Conversation.Type != ConversationType.Friendship
                ).ToListAsync();

            if (conversationMembers.Count >= 3)
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("conversation", "User cannot own more than 3 conversations.")
                .Build();
            }

            if (conversationMembers.Any(cm => cm.Conversation.Type == request.Type))
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("conversation", "User cannot own more than 1 conversations that same type.")
                .Build();
            }
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Conversation conversation = new()
                {
                    ConversationIndex = request.ConversationIndex,
                    Name = request.Name,
                    Type = request.Type,
                    Status = ConversationStatus.Active,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                };

                ConversationMember currentUser = new()
                {
                    UserId = request.MaskedAvatarId is null ? user.Id : null,
                    Role = ConversationMemberRole.Owner,
                    Status = ConversationMemberStatus.Joined,
                    JoinedAt = currentTime,
                    UpdatedAt = currentTime,
                };
                Message sysMsg = new()
                {
                    Conversation = conversation,
                    Type = MessageType.System,
                    CreatedAt = currentTime,
                };

                if (request.MaskedAvatarId is not null)
                {
                    UserMasked userMasked = new()
                    {
                        MaskedAvatarId = (Guid)request.MaskedAvatarId,
                        UserId = user.Id,
                        Status = UserMaskedStatus.Active,
                        CreatedAt = currentTime,
                    };

                    currentUser.UserMasked = userMasked;

                    var maskedAvatar = await context.MaskedAvatars.FirstOrDefaultAsync(ma => ma.Id == request.MaskedAvatarId);
                    sysMsg.Content = $"{maskedAvatar!.MaskedName} created the group";
                    conversation.GroupImageUrl = maskedAvatar.AvatarUrl;
                } else
                {
                    sysMsg.Content = $"{user.UserName} created the group";
                    if (user.AvatarUrl != null)
                    {
                        conversation.GroupImageUrl = user.AvatarUrl;
                    }
                }

                currentUser.Conversation = conversation;
                await context.ConversationMembers.AddAsync(currentUser);
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

        return Created();
    }
}
