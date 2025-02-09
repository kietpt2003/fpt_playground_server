using FPTPlaygroundServer.Common.Exceptions;

namespace FPTPlaygroundServer.Extensions;

public static class ExceptionHandlerExtensions
{
    public static void UseFPTPlaygroundExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<FPTPlaygroundExceptionHandler>();
    }
}
