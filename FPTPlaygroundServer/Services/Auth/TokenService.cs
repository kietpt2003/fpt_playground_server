using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.Auth;

public class TokenService(IOptions<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));

    public string CreateToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new("TokenClaim", "ForVerifyOnly")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(30),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new("RFTokenClaim", "ForVerifyOnly")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User?> ValidateRefreshToken(string token, AppDbContext context)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Missing Token")
                .Build();
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Extract the UserInfo claim
            var userId = principal.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Don't have user info in Token.")
                .Build();

            var checkClaim = principal.Claims.FirstOrDefault(c => c.Type == "RFTokenClaim" && c.Value == "ForVerifyOnly")?.Value;

            if (string.IsNullOrEmpty(checkClaim))
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Missing validation info in Token.")
                .Build();

            return await context.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("token", "Token invalid.")
                .Build();
        }
    }
}
