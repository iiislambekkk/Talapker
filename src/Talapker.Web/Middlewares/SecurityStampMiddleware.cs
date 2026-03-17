using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Web.Middlewares;

public class SecurityStampMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityStampMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, ILogger<SecurityStampMiddleware> logger)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip for auth endpoints and well-known
        if (path.StartsWith("/connect/") || 
            path.StartsWith("/.well-known/") ||
            path.StartsWith("/api/auth/") && !path.StartsWith("/api/auth/verify"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var stampFromToken = context.User.FindFirstValue("stamp");
            var userId = context.User.FindFirstValue(OpenIddictConstants.Claims.Subject);

            logger.LogInformation("[StampMiddleware] UserId={UserId}, StampFromToken={Stamp}",
                userId, stampFromToken);

            if (stampFromToken != null && userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                logger.LogInformation("[StampMiddleware] StampFromDB={StampDB}, Match={Match}",
                    user?.SecurityStamp, user?.SecurityStamp == stampFromToken);

                if (user?.SecurityStamp != stampFromToken)
                {
                    logger.LogWarning("[StampMiddleware] STALE SESSION for user {UserId}", userId);
                    context.Response.StatusCode = 401;
                    context.Response.Headers["Content-Type"] = "application/json";
                    await context.Response.WriteAsJsonAsync(new { error = "SESSION_STALE" });
                    return;
                }
            }
        }

        await _next(context);
    }
}