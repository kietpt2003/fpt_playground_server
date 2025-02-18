using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Headers;
using FPTPlaygroundServer.Features.Auth.Models;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using FPTPlaygroundServer.Features.Auth.Mappers;
using FPTPlaygroundServer.Common.Filters;
using FluentValidation;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[RequestValidation<Request>]
public class LoginGoogleController : ControllerBase
{
    public new record Request(Guid ServerId, string? DeviceToken);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.ServerId)
                .NotEmpty()
                .WithMessage("ServerId cannot be empty");
        }
    }

    [HttpPost("auth/google/{accessToken}")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Google login account",
        Description = "This API is for account login with Google. Note:" +
                            "<br>&nbsp; - Account bị Inactive thì vẫn Login GG vô được (Vì liên quan đến tiền trong ví)."
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromRoute] string accessToken, AppDbContext context, [FromServices] TokenService tokenService)
    {
        try
        {
            using var client = new HttpClient();
            // Set the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Make the GET request
            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={accessToken}");

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            var ggResponse = JsonConvert.DeserializeObject<LoginGoogleResponse>(responseContent);
            var account = await context.Accounts
                .Where(a => a.Email == ggResponse!.Email)
                .FirstOrDefaultAsync();

            var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("server", "Server not exist")
                .Build();

            //TH Mail chưa tồn tại
            if (account == null)
            {
                var registerAccountRequest = new RegisterAccountRequest
                {
                    Email = ggResponse!.Email,
                    Role = Role.User,
                    FullName = ggResponse.Name,
                    AvatarUrl = ggResponse.Picture,
                    LoginMethod = LoginMethod.Google,
                    Status = AccountStatus.Active,
                };
                if (!string.IsNullOrEmpty(request.DeviceToken))
                {
                    registerAccountRequest.Devices.Add(new Device
                    {
                        Token = request.DeviceToken!,
                    });
                }
                context.Accounts.Add(registerAccountRequest.ToAccountRequest()!);
                await context.SaveChangesAsync();

                TokenRequest tokenRequest = new() { Email = ggResponse!.Email };
                string token = tokenService.CreateToken(tokenRequest);
                string refreshToken = tokenService.CreateRefreshToken(tokenRequest);

                return Ok(new TokenResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                });
            }

            //TH đăng nhập bằng phương thức GG
            account = await context.Accounts
                .Where(u => u.Email == ggResponse!.Email && u.LoginMethod == LoginMethod.Google)
                .FirstOrDefaultAsync();
            if (account != null)
            {
                if (!string.IsNullOrEmpty(request.DeviceToken) && !account.Devices.Any(d => d.Token == request.DeviceToken))
                {
                    context.Devices.Add(new Device
                    {
                        AccountId = account.Id,
                        Token = request.DeviceToken!,
                    });
                    await context.SaveChangesAsync();
                }

                var user = await context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.AccountId == account.Id && u.ServerId == request.ServerId);

                string token = "";
                string refreshToken = "";

                if (user != null)
                {
                    token = tokenService.CreateToken(user.ToTokenRequest()!);
                    refreshToken = tokenService.CreateRefreshToken(user.ToTokenRequest()!);
                }
                else
                {
                    TokenRequest tokenRequest = new() { Email = ggResponse!.Email };
                    token = tokenService.CreateToken(tokenRequest);
                    refreshToken = tokenService.CreateRefreshToken(tokenRequest);
                }

                return Ok(new TokenResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                });
            }

            //TH mail này LoginMethod.Default
            account = await context.Accounts
                .Where(u => u.Email == ggResponse!.Email && u.LoginMethod == LoginMethod.Default)
                .FirstOrDefaultAsync();
            if (account != null)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("google", "This account doesn't signin by Google")
                    .Build();
            }

            throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_02)
                    .AddReason("google", "Unknown error")
                    .Build();
        }
        catch (HttpRequestException ex)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("google", ex.Message)
                .Build();
        }
        catch (JsonException ex)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("google", ex.Message)
                .Build();
        }
    }
}
