using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shortly.Infrastructure.Middleware;

/// <summary>
/// Middleware to measure request latency, expose timing headers, and log slow requests.
/// </summary>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // OnStarting executes callback just before the response headers are sent to the client.
        // It is necessary to set the headers here since we cannot modify response headers after writing response body content.
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Append X-Response-Time header showing response duration in ms.
            context.Response.Headers.Append("X-Response-Time", $"{elapsedMs}ms");

            // If the request took longer than 500ms, emit a dedicated slow-request log warning.
            if (elapsedMs > 500)
            {
                _logger.LogWarning("SLOW REQUEST: {Method} {Path} returned status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs);
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
