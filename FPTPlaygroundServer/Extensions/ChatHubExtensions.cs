using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Features.Chats;
using FPTPlaygroundServer.Services.Chats;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FPTPlaygroundServer.Extensions;

public static class ChatHubExtensions
{
    public static void UseChatHubHandler(this IEndpointRouteBuilder app)
    {
        app.MapHub<ChatHub>("chat/hub");
    }

    public static void AddSignalRService(this IServiceCollection services)
    {
        services.AddSignalR();
    }

    public static void AddAuthenticationForSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWT").Get<JwtSettings>();
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings!.SigningKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/group-chat/hub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }

    public static void AddAuthorizationForSignalR(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RoleRestricted", policy =>
                policy.Requirements.Add(new RoleRestrictHubRequirement()));
        });
    }

    public static void AddSingletonForSignalR(this IServiceCollection services)
    {
        services.AddSingleton<IUserIdProvider, ChatService>();
    }
}
