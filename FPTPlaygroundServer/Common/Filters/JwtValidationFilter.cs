using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FPTPlaygroundServer.Common.Filters;

[AttributeUsage(AttributeTargets.All)]
public class JwtValidationFilter : Attribute, IAsyncAuthorizationFilter
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var jwtSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<JwtSettings>>().Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));

        if (!context.HttpContext.Request.Headers.TryGetValue(AuthorizationHeader, out var authHeader) ||
            !authHeader.ToString().StartsWith(BearerPrefix))
        {
            context.Result = CreateErrorResult(FPTPlaygroundErrorCode.FPA_00, "Token require");
            return Task.CompletedTask;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        string token;
        try
        {
            token = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        }
        catch (Exception)
        {
            context.Result = CreateErrorResult(FPTPlaygroundErrorCode.FPB_02, "Token incorrect.");
            return Task.CompletedTask;
        }

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;
            if (string.IsNullOrEmpty(userInfoJson))
            {
                context.Result = CreateErrorResult(FPTPlaygroundErrorCode.FPA_00, "No user information in Token code.");
                return Task.CompletedTask;
            }

            var checkClaim = principal.Claims.FirstOrDefault(c => c.Type == "TokenClaim" && c.Value == "ForVerifyOnly")?.Value;
            if (string.IsNullOrEmpty(checkClaim))
            {
                context.Result = CreateErrorResult(FPTPlaygroundErrorCode.FPA_00, "Missing authentication information in Token code.");
                return Task.CompletedTask;
            }

            context.HttpContext.User = principal;
        }
        catch (SecurityTokenException)
        {
            context.Result = CreateErrorResult(FPTPlaygroundErrorCode.FPB_02, "Token incorrect or expired.");
        }

        return Task.CompletedTask;
    }

    private static IActionResult CreateErrorResult(FPTPlaygroundErrorCode errorCode, string message)
    {
        var reason = new Reason("token", message);
        var reasons = new List<Reason> { reason };
        var errorResponse = new FPTPlaygroundErrorResponse
        {
            Code = errorCode.Code,
            Title = errorCode.Title,
            Reasons = reasons
        };
        return new JsonResult(errorResponse) { StatusCode = (int)errorCode.Status };
    }
}
