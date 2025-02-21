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
using FPTPlaygroundServer.Services.Auth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FPTPlaygroundServer.Features.Auth.Models;
using FPTPlaygroundServer.Features.Auth.Mappers;

namespace FPTPlaygroundServer.Services.Auth;

public class TokenService(IOptions<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));

    public string CreateToken(TokenRequest tokenRequest)
    {
        var userInfoJson = JsonConvert.SerializeObject(tokenRequest, new StringEnumConverter());

        var claims = new List<Claim>
        {
            new("UserInfo", userInfoJson),
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

    public string CreateRefreshToken(TokenRequest tokenRequest)
    {
        var userInfoJson = JsonConvert.SerializeObject(tokenRequest, new StringEnumConverter());

        var claims = new List<Claim>
        {
            new("UserInfo", userInfoJson),
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

    public async Task<TokenResponse?> ValidateRefreshToken(string token, AppDbContext context)
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
            var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

            if (string.IsNullOrEmpty(userInfoJson))
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
            // Deserialize the custom user info object
            var tokenInfo = JsonConvert.DeserializeObject<TokenRequest>(userInfoJson);

            var user = await context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Id == tokenInfo!.UserId);

            string tokenResponse = "";
            string refreshToken = "";

            if (user != null)
            {
                tokenResponse = CreateToken(user.ToTokenRequest()!);
                refreshToken = CreateRefreshToken(user.ToTokenRequest()!);
            }
            else
            {
                TokenRequest tokenRequest = new() { 
                    Email = tokenInfo!.Email,
                    Role = Role.User
                };
                tokenResponse = CreateToken(tokenRequest);
                refreshToken = CreateRefreshToken(tokenRequest);
            }

            return new TokenResponse
            {
                Token = tokenResponse,
                RefreshToken = refreshToken
            };
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
