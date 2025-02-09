using Microsoft.IdentityModel.Tokens;

namespace FPTPlaygroundServer.Common.Exceptions;

public class FPTPlaygroundExceptionHandler(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<FPTPlaygroundExceptionHandler> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex, context, logger);
        }
    }

    private static async Task HandleExceptionAsync(Exception ex, HttpContext context, ILogger logger)
    {
        if (ex is FPTPlaygroundException fptPlagroundException)
        {
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = fptPlagroundException.ErrorCode.Code,
                Title = fptPlagroundException.ErrorCode.Title,
                Reasons = fptPlagroundException.GetReasons().Select(reason => new Reason(reason.Title, reason.ReasonMessage)).ToList()
            };

            context.Response.StatusCode = (int)fptPlagroundException.ErrorCode.Status;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        else if (ex is SecurityTokenException) // JWT-specific exception
        {
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPB_03.Code,
                Title = FPTPlaygroundErrorCode.FPB_03.Title,
                Reasons = [
                    new Reason("token", "Invalid token.")!
                ]
            };
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        else if (ex is UnauthorizedAccessException)
        {
            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPA_00.Code,
                Title = FPTPlaygroundErrorCode.FPA_00.Title,
                Reasons = new List<Reason>{
                    new Reason("access", "Access deny.")!
                }
            };
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
        else
        {
            logger.LogError(ex.Message);

            var errorResponse = new FPTPlaygroundErrorResponse
            {
                Code = FPTPlaygroundErrorCode.FPS_00.Code,
                Title = FPTPlaygroundErrorCode.FPS_00.Title,
                Reasons = new List<Reason>{
                    new Reason("server", "Unexpected error.")!
                }
            };
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
