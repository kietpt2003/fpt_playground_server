using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FPTPlaygroundServer.Common.Exceptions;

namespace FPTPlaygroundServer.Extensions;

public static class RateLimiterExtensions
{
    public static void AddFPTPlaygroundRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddConcurrencyLimiter("concurrency", options =>
            {
                options.PermitLimit = 20;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 10;
            });
        });
    }

    public static void UseFPTPlaygroundRateLimiter(this IApplicationBuilder app)
    {
        app.UseMiddleware<RateLimitExceptionMiddleware>();
        app.UseRateLimiter();
    }
}

public class RateLimitExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);

        if (httpContext.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_02)
                .AddReason("rateLimit", "Too many request")
                .Build();
        }
    }
}
