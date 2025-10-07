using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Middleware;

namespace Server.Helper
{
    public static class ExceptionLoggerHelper
    {
        /// <summary>
        /// Log exception với format chi tiết
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="exception">Exception cần log</param>
        /// <param name="context">Context thêm (ví dụ: user ID, request path)</param>
        public static void LogException(ILogger logger, Exception exception, Dictionary<string, object>? context = null)
        {
            var logLevel = GetLogLevel(exception);
            var message = BuildLogMessage(exception, context);
            
            logger.Log(logLevel, exception, message);
        }

        /// <summary>
        /// Xác định log level dựa trên loại exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Log level phù hợp</returns>
        private static LogLevel GetLogLevel(Exception exception)
        {
            return exception switch
            {
                // Critical errors - cần attention ngay lập tức
                OutOfMemoryException => LogLevel.Critical,
                StackOverflowException => LogLevel.Critical,
                
                // Errors - lỗi nghiêm trọng nhưng có thể phục hồi
                System.Data.Common.DbException => LogLevel.Error,
                TimeoutException => LogLevel.Error,
                
                // Warnings - lỗi do người dùng hoặc business logic
                BusinessLogicException => LogLevel.Warning,
                ConflictException => LogLevel.Warning,
                
                // Information - lỗi thông thường, có thể expect
                NotFoundException => LogLevel.Information,
                UnauthorizedException => LogLevel.Information,
                ValidationException => LogLevel.Information,
                ArgumentException => LogLevel.Information,
                
                // Default
                _ => LogLevel.Error
            };
        }

        /// <summary>
        /// Tạo log message chi tiết
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="context">Context thêm</param>
        /// <returns>Log message</returns>
        private static string BuildLogMessage(Exception exception, Dictionary<string, object>? context)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"Exception Type: {exception.GetType().Name}");
            sb.AppendLine($"Message: {exception.Message}");
            
            // Thêm context nếu có
            if (context != null && context.Count > 0)
            {
                sb.AppendLine("Context:");
                foreach (var kvp in context)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
            
            // Thêm details cho custom exceptions
            if (exception is AppException appException && appException.Details.Count > 0)
            {
                sb.AppendLine("Details:");
                foreach (var detail in appException.Details)
                {
                    sb.AppendLine($"  - {detail}");
                }
            }
            
            // Thêm inner exception nếu có
            if (exception.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {exception.InnerException.GetType().Name}");
                sb.AppendLine($"Inner Message: {exception.InnerException.Message}");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Log exception với thông tin HTTP context
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="exception">Exception</param>
        /// <param name="httpMethod">HTTP method</param>
        /// <param name="path">Request path</param>
        /// <param name="userId">User ID nếu có</param>
        /// <param name="userAgent">User agent nếu có</param>
        public static void LogHttpException(
            ILogger logger, 
            Exception exception, 
            string? httpMethod = null,
            string? path = null,
            string? userId = null,
            string? userAgent = null)
        {
            var context = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(httpMethod))
                context["HttpMethod"] = httpMethod;
                
            if (!string.IsNullOrEmpty(path))
                context["RequestPath"] = path;
                
            if (!string.IsNullOrEmpty(userId))
                context["UserId"] = userId;
                
            if (!string.IsNullOrEmpty(userAgent))
                context["UserAgent"] = userAgent;
            
            LogException(logger, exception, context);
        }

        /// <summary>
        /// Tạo correlation ID để track request
        /// </summary>
        /// <returns>Correlation ID</returns>
        public static string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString("N")[..8];
        }
        
        /// <summary>
        /// Kiểm tra xem có nên log stack trace hay không
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>True nếu nên log stack trace</returns>
        public static bool ShouldLogStackTrace(Exception exception)
        {
            return exception switch
            {
                // Không log stack trace cho các lỗi thông thường
                NotFoundException => false,
                UnauthorizedException => false,
                ValidationException => false,
                ArgumentException => false,
                
                // Log stack trace cho các lỗi nghiêm trọng
                _ => true
            };
        }
    }
}