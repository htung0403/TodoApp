using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Server.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N")[..8];
            context.Items["CorrelationId"] = correlationId;

            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            
            // Log incoming request
            var requestInfo = new StringBuilder();
            requestInfo.AppendLine($"[{correlationId}] Incoming Request:");
            requestInfo.AppendLine($"  Method: {request.Method}");
            requestInfo.AppendLine($"  Path: {request.Path}");
            requestInfo.AppendLine($"  QueryString: {request.QueryString}");
            requestInfo.AppendLine($"  User-Agent: {request.Headers["User-Agent"]}");
            
            // Log user info if authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("userId")?.Value;
                var username = context.User.FindFirst("username")?.Value;
                requestInfo.AppendLine($"  User: {username} (ID: {userId})");
            }

            _logger.LogInformation(requestInfo.ToString());

            // Execute the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response
            var response = context.Response;
            var responseInfo = new StringBuilder();
            responseInfo.AppendLine($"[{correlationId}] Outgoing Response:");
            responseInfo.AppendLine($"  Status Code: {response.StatusCode}");
            responseInfo.AppendLine($"  Content Type: {response.ContentType}");
            responseInfo.AppendLine($"  Duration: {stopwatch.ElapsedMilliseconds}ms");

            var logLevel = GetLogLevel(response.StatusCode);
            _logger.Log(logLevel, responseInfo.ToString());
        }

        private static LogLevel GetLogLevel(int statusCode)
        {
            return statusCode switch
            {
                >= 200 and < 300 => LogLevel.Information,
                >= 300 and < 400 => LogLevel.Information,
                >= 400 and < 500 => LogLevel.Warning,
                >= 500 => LogLevel.Error,
                _ => LogLevel.Information
            };
        }
    }

    // Extension method
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}