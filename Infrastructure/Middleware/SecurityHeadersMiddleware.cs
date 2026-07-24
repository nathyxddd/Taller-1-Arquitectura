using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Shortly.Infrastructure.Middleware;

/// <summary>
/// Middleware to enforce baseline browser-side defenses on every HTTP response.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Strict-Transport-Security: Forces HTTPS to mitigate Man-in-the-Middle (MitM) / SSL stripping attacks.
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // X-Content-Type-Options: Prevents MIME-sniffing vulnerability by forcing the declared Content-Type.
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Protects against Clickjacking attacks by blocking the page from being loaded in iframes.
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Referrer-Policy: Prevents leaking sensitive URL parameters or routing data to third-party domains.
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Restricts browser API access (camera, location, mic) to limit the attack surface.
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}
