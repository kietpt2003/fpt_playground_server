using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FPTPlaygroundServer.Features.Account;

[ApiController]
public class GetCurrentAccountController : ControllerBase
{
    [HttpGet("accounts/current")]
    [Tags("Accounts")]
    [SwaggerOperation(Summary = "Get Current Account", Description = "This API is for getting the current authenticated account")]
    public async Task<IActionResult> Handler([FromServices] CurrentUserService currentUserService, [FromServices] AppDbContext context)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == "tuankiet29012003@gmail.com");
        //var user = await currentUserService.GetCurrentUser();
        return Ok(account.Email);
    }
}
