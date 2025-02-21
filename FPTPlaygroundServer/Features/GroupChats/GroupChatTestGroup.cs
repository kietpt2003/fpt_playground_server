using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.GroupChats;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GroupChatTestGroup(IHubContext<GroupChatHub> hub) : ControllerBase
{
    public new class Request
    {
        public string TestMessasge { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(sp => sp.TestMessasge)
                .NotEmpty()
                .WithMessage("Message cannot be empty");
        }
    }

    [HttpPost("group-chat/group")]
    [Tags("Test Chat")]
    [SwaggerOperation(
        Summary = "Test Sending Group Chat",
        Description = "This API is for testing sending group message. Note: " +
                            "<br>&nbsp; - Dùng API này để test gửi group message để FE nhận message."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, AppDbContext context, [FromServices] CurrentUserService currentUserService)
    {
        var currentUser = await currentUserService.GetCurrentUser();

        switch (currentUser!.Account.Role)
        {
            case Role.Admin:
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_01)
                .AddReason("groupChat", "Admin cannot send chat.")
                .Build();
            case Role.User:
                await hub.Clients.Group("UserGroup").SendAsync("GroupMethod", request.TestMessasge);
                break;
            default:
                break;
        }

        return Ok();
    }
}
