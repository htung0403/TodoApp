using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Helper;

namespace Server.Middleware
{
    // Custom Exceptions
    public abstract class AppException : Exception
    {
        public List<string> Details { get; }
        protected AppException(string message) : base(message) => Details = new List<string>();
        protected AppException(string message, List<string> details) : base(message) => Details = details ?? new List<string>();
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string resourceName, object key) : base($"{resourceName} with key '{key}' was not found") { }
    }

    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, List<string> errors) : base(message, errors) { }
        public ValidationException(List<string> errors) : base("Validation failed", errors) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, List<string> details) : base(message, details) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException() : base("You are not authorized to perform this action") { }
        public UnauthorizedException(string message) : base(message) { }
    }

    public class BusinessLogicException : AppException
    {
        public BusinessLogicException(string message) : base(message) { }
        public BusinessLogicException(string message, List<string> details) : base(message, details) { }
    }

    // Middleware chính
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = Guid.NewGuid().ToString("N")[..8];
            context.Items["CorrelationId"] = correlationId;

            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception occurred: {Message} | CorrelationId: {CorrelationId}", 
                    exception.Message, correlationId);
                await HandleExceptionAsync(context, exception, correlationId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var (statusCode, response) = exception switch
            {
                NotFoundException ex => (HttpStatusCode.NotFound, 
                    ApiResponseHelper.NotFound(ex.Message)),

                ValidationException ex => (HttpStatusCode.BadRequest, 
                    ApiResponseHelper.ValidationError(ex.Details.Any() ? ex.Details : [ex.Message])),

                ConflictException ex => (HttpStatusCode.Conflict, 
                    ApiResponseHelper.Error(ex.Message, ex.Details.Any() ? ex.Details : [ex.Message])),

                UnauthorizedException ex => (HttpStatusCode.Unauthorized, 
                    ApiResponseHelper.Unauthorized(ex.Message)),

                BusinessLogicException ex => (HttpStatusCode.UnprocessableEntity, 
                    ApiResponseHelper.Error(ex.Message, ex.Details.Any() ? ex.Details : [ex.Message])),

                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, 
                    ApiResponseHelper.Unauthorized("Access denied")),

                KeyNotFoundException => (HttpStatusCode.NotFound, 
                    ApiResponseHelper.NotFound(exception.Message)),

                ArgumentException => (HttpStatusCode.BadRequest, 
                    ApiResponseHelper.Error(exception.Message)),

                DbUpdateException => (HttpStatusCode.Conflict, 
                    ApiResponseHelper.Error("Database operation failed", ["Data conflict occurred"])),

                TimeoutException => (HttpStatusCode.RequestTimeout, 
                    ApiResponseHelper.Error("Request timeout", ["Operation took too long"])),

                _ => (HttpStatusCode.InternalServerError, 
                    CreateInternalServerErrorResponse(exception))
            };

            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static object CreateInternalServerErrorResponse(Exception exception)
        {
#if DEBUG
            return ApiResponseHelper.Error(exception.Message, [exception.StackTrace ?? "No stack trace"]);
#else
            return ApiResponseHelper.Error("An internal server error occurred", ["Something went wrong"]);
#endif
        }
    }

    // Extension method
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}