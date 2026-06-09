using System.Diagnostics;

namespace FiapCloudGames.API.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        var start = Stopwatch.GetTimestamp();

        await _next(context);

        var elapsed = (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency;
        _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Duration}ms", method, path, context.Response.StatusCode, elapsed);
    }
}
