using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Auth;
using FPTPlaygroundServer.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FPTPlaygroundServer.Common.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RolesFilterAttributeparams(Role[] acceptedRoles) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<CurrentUserService>();
        var user = await currentUserService.GetCurrentUser();

        if (user != null && acceptedRoles.Contains(user.Account.Role))
        {
            await next();
        }
        else
        {
            var reason = new Reason("role", "Account is not authorized to access this API.");
            var reasons = new List<Reason> { reason };
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPA_01.Code,
                Title = FPTPlaygroundErrorCode.FPA_01.Title,
                Reasons = reasons
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = (int)FPTPlaygroundErrorCode.FPA_01.Status
            };
        }
    }
}
