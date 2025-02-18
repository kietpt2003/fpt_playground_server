using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FPTPlaygroundServer.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using FPTPlaygroundServer.Features.Auth.Mappers;
using FPTPlaygroundServer.Services.Auth.Models;
using FPTPlaygroundServer.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json.Linq;

namespace FPTPlaygroundServer.Features.Auth;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class ChangeServer : ControllerBase
{
    public new record Request(Guid ServerId);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.ServerId)
                .NotEmpty()
                .WithMessage("ServerId cannot be empty");
        }

    }

    [HttpPost("auth/change-server")]
    [Tags("Auth")]
    [SwaggerOperation(
        Summary = "Change server",
        Description = "This API is for change server"
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService, [FromServices] CurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));
        var userRequest = httpContextAccessor.HttpContext?.Request;
        var authHeader = userRequest?.Headers.Authorization.ToString();
        var token = authHeader?.Replace("Bearer ", string.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidIssuer = jwtSettings.Value.Issuer,
            ValidAudience = jwtSettings.Value.Audience,
            ClockSkew = TimeSpan.Zero
        };

        if (string.IsNullOrEmpty(token))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Missing Token")
                .Build();
        }

        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

        if (string.IsNullOrEmpty(userInfoJson))
            throw FPTPlaygroundException.NewBuilder()
            .WithCode(FPTPlaygroundErrorCode.FPA_00)
            .AddReason("token", "Don't have user info in Token.")
            .Build();

        var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
            .WithCode(FPTPlaygroundErrorCode.FPB_02)
            .AddReason("server", "Server not exist")
            .Build();

        var jObject = JObject.Parse(userInfoJson);

        string tokenResponse = "";
        string refreshToken = "";

        if (jObject["UserId"]?.Type != JTokenType.Null) //TH có userId trong token thì phải check coi userId này có nằm cùng server không, phòng TH truyền vào token có userId thì cũng nhả ra lại token
        {
            var user = await context.Users
               .Include(u => u.Account)
               .FirstOrDefaultAsync(u => u.Id == jObject["UserId"]!.ToObject<Guid?>() && u.ServerId == request.ServerId);
            if (user == null) //TH nếu không có user (có userId hoặc không có userId nhưng không có user trong server đó) => trả về token không có userId
            {
                TokenRequest tokenRequest = new() { Email = jObject["Email"]!.ToString() };
                tokenResponse = tokenService.CreateToken(tokenRequest);
                refreshToken = tokenService.CreateRefreshToken(tokenRequest);
            }
            else //TH có user trong server đó, tức là user truyền token có userId và truyền lại serverId có user => trả lại token có userId
            {
                tokenResponse = tokenService.CreateToken(user.ToTokenRequest()!);
                refreshToken = tokenService.CreateRefreshToken(user.ToTokenRequest()!);
            }

            return Ok(new TokenResponse
            {
                Token = tokenResponse,
                RefreshToken = refreshToken
            });
        }
        else //TH không có userId trong Token tức là phải lấy account theo email
        {
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == jObject["Email"]!.ToString());
            if (account!.Status != AccountStatus.Active)
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("account", "Account has been blocked or in pending state.")
                .Build();
            }
            var serverUser = await context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.AccountId == account.Id && u.ServerId == request.ServerId);

            if (serverUser == null) //TH đổi sang server khác mà chưa có user thì trả về token ko có userId để vô trang RegisterUser
            {
                TokenRequest tokenRequest = new() { Email = account.Email };
                tokenResponse = tokenService.CreateToken(tokenRequest);
                refreshToken = tokenService.CreateRefreshToken(tokenRequest);
            }
            else
            {
                tokenResponse = tokenService.CreateToken(serverUser.ToTokenRequest()!);
                refreshToken = tokenService.CreateRefreshToken(serverUser.ToTokenRequest()!);
            }

            return Ok(new TokenResponse
            {
                Token = tokenResponse,
                RefreshToken = refreshToken
            });
        }


    }
}
