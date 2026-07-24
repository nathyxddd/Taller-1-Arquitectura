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
        // 1. Strict-Transport-Security (HSTS)
        // Concept: Forces browsers to only interact with the server using secure HTTPS connections.
        // Mitigation: Prevents Man-in-the-Middle (MitM) attacks such as SSL stripping.
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // 2. X-Content-Type-Options
        // Concept: Disables MIME type sniffing.
        // Mitigation: Prevents cross-site scripting (XSS) and drive-by downloads by forcing the browser to adhere strictly to the Content-Type header.
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 3. X-Frame-Options
        // Concept: Indicates whether the browser should be allowed to render a page in a <frame>, <iframe>, <embed> or <object>.
        // Mitigation: Protects against Clickjacking attacks where an attacker overlays their malicious UI on top of a trusted page.
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // 4. Referrer-Policy
        // Concept: Controls how much referrer information is included in the Referer header of requests.
        // Mitigation: Protects user privacy and prevents the leakage of sensitive URL structures/parameters to third-party domains.
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // 5. Permissions-Policy
        // Concept: Restricts browser APIs and features (like geolocation, microphone, camera, etc.) that the page can access.
        // Mitigation: Limits the attack surface by ensuring third-party scripts or cross-origin frames cannot access hardware/APIs without authorization.
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}
