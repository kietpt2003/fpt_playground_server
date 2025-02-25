using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using FPTPlaygroundServer.Services.Auth.Models;

namespace FPTPlaygroundServer.Common.Filters;

public class RoleRestrictHubRequirement : AuthorizationHandler<RoleRestrictHubRequirement, HubInvocationContext>,
    IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        RoleRestrictHubRequirement requirement,
        HubInvocationContext resource)
    {
        var userInfoJson = context.User!.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

        var userInfo = JsonConvert.DeserializeObject<TokenRequest>(userInfoJson!);

        if (userInfo != null && IsUserAllowedToDoThis(resource.HubMethodName, userInfo.Role.ToString()))
        {
            context.Succeed(requirement);

        }
        return Task.CompletedTask;
    }

    private static bool IsUserAllowedToDoThis(string hubMethodName, string userRole)
    {
        if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        else if ((userRole.Equals("User", StringComparison.OrdinalIgnoreCase)) && hubMethodName.Equals("JoinGroup", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
