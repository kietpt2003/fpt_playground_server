using Microsoft.AspNetCore.Mvc;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using FluentValidation;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Common.Exceptions;

namespace FPTPlaygroundServer.Features.GroupChats;

[ApiController]
[JwtValidationFilter]
[RolesFilter(Role.User)]
[RequestValidation<Request>]
public class GroupChatTestCurrent(IHubContext<GroupChatHub> hub) : ControllerBase
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

    [HttpPost("group-chat/current")]
    [Tags("Test Chat")]
    [SwaggerOperation(
        Summary = "Test Sending Personal Chat",
        Description = "This API is for testing sending personal message. Note: " +
                            "<br>&nbsp; - Dùng API này để test gửi personal message để FE nhận message."
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
                await hub.Clients.User(currentUser.Id.ToString()).SendAsync("PersonalMethod", request.TestMessasge);
                break;
            default:
                break;
        }

        return Ok();
    }
}
