using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

        if (string.IsNullOrEmpty(token))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Missing Token")
                .Build();
        }

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var userId = principal.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Don't have user info in Token.")
                .Build();

            return await context.Users
                .Include(u => u.Account)
                .Include(u => u.Specialize)
                .Include(u => u.Server)
                .Include(u => u.CoinWallet)
                .Include(u => u.DiamondWallet)
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
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
