namespace SparkLabs.ProfileApi.Auth;

/// <summary>
/// DEVELOPMENT ONLY: Simulates authentication by reading user ID from a header.
///
/// In production, this would be replaced with proper JWT authentication middleware
/// that extracts the user ID from validated token claims.
///
/// Usage: Add "X-Impersonate-User: {userId}" header to requests.
/// </summary>
public class ImpersonationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ImpersonationMiddleware> _logger;

    public const string HeaderName = "X-Impersonate-User";

    public ImpersonationMiddleware(RequestDelegate next, ILogger<ImpersonationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var userIdHeader)
            && Guid.TryParse(userIdHeader, out var userId))
        {
            userContext.UserId = userId;
            _logger.LogDebug("Impersonating user: {UserId}", userId);
        }

        await _next(context);
    }
}

public static class ImpersonationMiddlewareExtensions
{
    /// <summary>
    /// Adds impersonation middleware for development/testing.
    /// DO NOT use in production - replace with proper JWT authentication.
    /// </summary>
    public static IApplicationBuilder UseImpersonation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ImpersonationMiddleware>();
    }
}
