using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.FriendShips;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class ResponseFriendshipRequest : ControllerBase
{
    public new class Request
    {
        public Guid FriendId { get; set; }
        public FriendshipStatus Status { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Status)
                .Must(status => status != FriendshipStatus.Pending)
                .WithMessage("Status cannot be Pending");
            RuleFor(c => c.FriendId)
                .NotEmpty()
                .WithMessage("FriendId cannot be empty");
        }
    }

    [HttpPut("friendship")]
    [Tags("Friendships")]
    [SwaggerOperation(
        Summary = "Update Friendship",
        Description = """
        This API is for user change friendship to Accepted, Cancelled, Blocked or Unblocked. Note: 
        - Dùng api này để đồng ý hoặc từ chối hoặc block user, hoặc gỡ chặn user
        - Từ chối kết bạn hoặc gỡ chặn tức là xóa friendship
        - Dùng API này để block luôn cả TH đang là Mate
        """
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(
        [FromBody] Request request, AppDbContext context,
        [FromServices] CurrentUserService currentUserService,
        [FromServices] FCMNotificationService fcmNotificationService
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

        var friend = await context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.Id == request.FriendId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Friend not found")
                .Build();

        if (friend!.Status == UserStatus.Inactive || friend.Account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("friend", "Your friend account have been inactive or not deactivate")
                .Build();
        }

        var friendship = await context.Friendships
                .Include(fs => fs.User)
                    .ThenInclude(u => u.Account)
                .Include(fs => fs.Friend)
                    .ThenInclude(u => u.Account)
                .FirstOrDefaultAsync(fs => (fs.UserId == user.Id && fs.FriendId == request.FriendId) || (fs.UserId == request.FriendId && fs.FriendId == user.Id));

        if (request.Status != FriendshipStatus.Blocked)
        {
            if (friendship is null)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "Friendship not found")
                    .Build();
            }

            if (friendship.Status == request.Status)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", "This friendship already in that state")
                    .Build();
            }

            var theirConversation = await context.Conversations
                .Include(c => c.ConversationMembers)
                    .ThenInclude(cm => cm.UserMasked)
                .Where(c => c.Type == ConversationType.Personal || c.Type == ConversationType.Dating || c.Type == ConversationType.Friendship)
                .Where(c => c.ConversationMembers.Any(cm => cm.UserId == friendship.FriendId || (cm.UserMasked != null && cm.UserMasked.UserId == friendship.FriendId)) &&
                    c.ConversationMembers.Any(cm => cm.UserId == friendship.UserId || (cm.UserMasked != null && cm.UserMasked.UserId == friendship.UserId))
                ).FirstOrDefaultAsync();

            if (request.Status == FriendshipStatus.Accepted && friendship.Status == FriendshipStatus.Pending)
            {
                if (friendship.UserId == user.Id)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPV_00)
                    .AddReason("friendship", "You cannot accept your request")
                    .Build();
                }
                try
                {
                    friendship.Status = FriendshipStatus.Accepted;
                    friendship.UpdatedAt = currentTime;
                    friendship.UpdatedBy = user.Id;

                    List<string> userDeviceTokens = friendship.Friend.Account.Devices.Select(d => d.Token).ToList(); //User sẽ là Friend vì người gửi là bạn của User
                    List<string> friendDeviceTokens = friendship.User.Account.Devices.Select(d => d.Token).ToList(); //Bạn của bạn trong lúc này chính là người gửi yêu cầu => họ là User

                    if (friendDeviceTokens.Count > 0)
                    {
                        await fcmNotificationService.SendMultibleNotificationAsync(
                            userDeviceTokens,
                            "Friend Accepted",
                            $"You and {friendship.Friend.UserName} are friends now!",
                            new Dictionary<string, string>()
                            {
                                { "friendShipId", friendship.Id.ToString() },
                            }
                        );
                    }
                    if (userDeviceTokens.Count > 0)
                    {
                        await fcmNotificationService.SendMultibleNotificationAsync(
                            userDeviceTokens,
                            "Friend Accepted",
                            $"You and {friendship.User.UserName} are friends now!",
                            new Dictionary<string, string>()
                            {
                                { "friendShipId", friendship.Id.ToString() },
                            }
                        );
                    }
                    var strategy = context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await context.Database.BeginTransactionAsync();
                        try
                        {
                            await context.Notifications.AddRangeAsync([new Notification
                            {
                                UserId = friendship.UserId,
                                Title = "Friend Accepted",
                                Content = $"You and {friendship.Friend.UserName} are friends now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Friendship
                            },
                             new Notification
                            {
                                UserId = friendship.FriendId,
                                Title = "Friend Accepted",
                                Content = $"You and {friendship.User.UserName} are friends now!",
                                CreatedAt = currentTime,
                                IsRead = false,
                                Type = NotificationType.Friendship
                            }]);

                            if (theirConversation != null) //Nếu có conversation rồi thì cập nhật lại Type thôi
                            {
                                foreach (var member in theirConversation.ConversationMembers)
                                {
                                    //Nếu đồng ý kết bạn thì thêm UserId và không xóa MaskedId
                                    if (!member.UserId.HasValue && member.UserMaskedId.HasValue && member.UserMasked!.UserId == friendship.UserId)
                                    {
                                        member.UserId = friendship.FriendId;
                                    }
                                    else if (!member.UserId.HasValue && member.UserMaskedId.HasValue && member.UserMasked!.UserId == friendship.FriendId)
                                    {
                                        member.UserId = friendship.UserId;
                                    }
                                }
                                theirConversation.Name = "Friendship Conversation";
                                theirConversation.Type = ConversationType.Friendship;
                            }
                            else //Nếu chưa có conversation thì tạo mới
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
                                    UserId = friendship.UserId,
                                    Role = ConversationMemberRole.Owner,
                                    Status = ConversationMemberStatus.Joined,
                                    JoinedAt = currentTime,
                                    UpdatedAt = currentTime,
                                };

                                ConversationMember currentReceiver = new()
                                {
                                    Conversation = conversation,
                                    UserId = friendship.FriendId,
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (request.Status == FriendshipStatus.Cancelled && (friendship.Status == FriendshipStatus.Pending || friendship.Status == FriendshipStatus.Accepted))
            {
                if (theirConversation != null)
                {
                    foreach (var member in theirConversation.ConversationMembers)
                    {
                        //Nếu hủy kết bạn thì xóa UserId và trở thành chat vs người lạ
                        if (member.UserId.HasValue)
                        {
                            member.UserId = null;
                        }
                    }
                    theirConversation.Type = ConversationType.Personal;
                }
                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync();
            }
            else if (request.Status == FriendshipStatus.Unblocked && friendship.Status == FriendshipStatus.Blocked)
            {
                if (friendship.UpdatedBy != user.Id)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPV_00)
                    .AddReason("friendship", "You cannot unblocked")
                    .Build();
                }

                if (theirConversation != null) //Nếu gỡ chặn thì chỉ đổi Type, không được xóa UserId vì có TH 2 người đã là (ko có UserMasked) mà xóa UserId => ko có cả 2 User hay Masked
                {
                    theirConversation.Name = "Personal Conversation";
                    theirConversation.Type = ConversationType.Personal;
                }

                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync();
            }
            else
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPV_00)
                    .AddReason("Syntax", "Please select correct Status")
                    .Build();
            }
        }
        else
        {
            if (friendship is null) //TH null tức là 2 người này chưa kết bạn mà muốn block luôn. Lưu ý, chưa kết bạn thì có thể đã chat ẩn danh rồi nên block thì vẫn có thể thấy block trong conversation được, chỉ là không được chat thôi
            {
                Friendship newFriendship = new()
                {
                    UserId = user.Id,
                    FriendId = request.FriendId,
                    Status = FriendshipStatus.Blocked,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    UpdatedBy = user.Id
                };
                await context.Friendships.AddAsync(newFriendship);

                await context.SaveChangesAsync();
            }
            else //TH 2 người này đã kb hoặc 2 người này hẹn hò (Lưu ý: không cần đổi conversation type vì để get ra thấy được block conversation, cho đến khi unblock tương đương với vô stranger xem)
            {
                if (friendship.Status == FriendshipStatus.Blocked)
                {
                    throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_03)
                    .AddReason("friendship", friendship.UpdatedBy == user.Id ? "This friendship already in that state" : "User have been blocked")
                    .Build();
                }
                else
                {
                    friendship.Status = FriendshipStatus.Blocked;
                    friendship.UpdatedAt = DateTime.UtcNow;
                    friendship.UpdatedBy = user.Id;

                    //Check xem coi có phải Mate không, nếu phải thì xóa Mate luôn
                    var currentDating = await context.Mates.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.MateId == friend.Id) || (m.MateId == user.Id && m.UserId == friend.Id));
                    if (currentDating != null)
                    {
                        context.Mates.Remove(currentDating);
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        return Ok();
    }
}
