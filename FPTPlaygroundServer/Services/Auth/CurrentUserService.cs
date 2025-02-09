using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FPTPlaygroundServer.Services.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings, AppDbContext context)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));

    public async Task<User?> GetCurrentUser()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        var authHeader = request?.Headers.Authorization.ToString();
        var token = authHeader?.Replace("Bearer ", string.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = _key,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };

        if (token == "")
        {
            return null;
        }

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

            var userInfo = JsonConvert.DeserializeObject<TokenRequest>(userInfoJson!);

            return await context.Users
                .Include(u => u.Account)
                .Include(u => u.Specialize)
                .Include(u => u.Server)
                .FirstOrDefaultAsync(x => x.Id == userInfo!.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<Guid> GetCurrentUserId()
    {
        var user = await GetCurrentUser() ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("user", "User not exist.")
                .Build();
        return user.Id;
    }
}
