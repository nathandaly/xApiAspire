namespace xApiApp.ApiService.Middleware;

public class XApiVersionMiddleware
{
    private readonly RequestDelegate _next;
    private const string XApiVersionHeader = "X-Experience-API-Version";
    private const string XApiConsistentThroughHeader = "X-Experience-API-Consistent-Through";
    private const string SupportedVersion = "1.0.3";

    public XApiVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip version header check for non-xAPI endpoints (health checks, root, etc.)
        var path = context.Request.Path;
        var isXApiEndpoint = path.StartsWithSegments("/xapi/");
        
        // Only require version header for xAPI endpoints (not health checks, root, or about)
        if (isXApiEndpoint && !path.StartsWithSegments("/xapi/about"))
        {
            var requestedVersion = context.Request.Headers[XApiVersionHeader].FirstOrDefault();
            
            if (string.IsNullOrEmpty(requestedVersion))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing X-Experience-API-Version header");
                return;
            }

            // Normalize version (accept "1.0" as "1.0.0")
            if (requestedVersion == "1.0")
            {
                requestedVersion = "1.0.0";
            }

            // Validate version
            if (!requestedVersion.StartsWith("1.0."))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Unsupported xAPI version: {requestedVersion}. Supported versions: 1.0.x");
                return;
            }
        }

        // Add version header to response
        context.Response.Headers[XApiVersionHeader] = SupportedVersion;
        
        // Add consistent through header for statement requests
        if (context.Request.Path.StartsWithSegments("/xapi/statements"))
        {
            context.Response.Headers[XApiConsistentThroughHeader] = DateTimeOffset.UtcNow.ToString("O");
        }

        await _next(context);
    }
}

public static class XApiVersionMiddlewareExtensions
{
    public static IApplicationBuilder UseXApiVersion(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XApiVersionMiddleware>();
    }
}

