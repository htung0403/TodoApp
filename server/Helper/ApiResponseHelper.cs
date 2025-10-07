using System;
using System.Collections.Generic;

namespace Server.Helper
{
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Tạo response thành công với data
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="data">Dữ liệu trả về</param>
        /// <param name="message">Thông điệp</param>
        /// <returns>ApiResponse với status success</returns>
        public static ApiResponse<T> Success<T>(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response thành công không có data
        /// </summary>
        /// <param name="message">Thông điệp</param>
        /// <returns>ApiResponse với status success</returns>
        public static ApiResponse<object> Success(string message = "Success")
        {
            return new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu</typeparam>
        /// <param name="message">Thông điệp lỗi</param>
        /// <param name="errors">Danh sách lỗi chi tiết</param>
        /// <returns>ApiResponse với status error</returns>
        public static ApiResponse<T> Error<T>(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi không có data
        /// </summary>
        /// <param name="message">Thông điệp lỗi</param>
        /// <param name="errors">Danh sách lỗi chi tiết</param>
        /// <returns>ApiResponse với status error</returns>
        public static ApiResponse<object> Error(string message, List<string>? errors = null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi validation
        /// </summary>
        /// <param name="errors">Danh sách lỗi validation</param>
        /// <returns>ApiResponse với status error</returns>
        public static ApiResponse<object> ValidationError(List<string> errors)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = null,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi unauthorized
        /// </summary>
        /// <param name="message">Thông điệp lỗi</param>
        /// <returns>ApiResponse với status error</returns>
        public static ApiResponse<object> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = new List<string> { "Access denied" },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi not found
        /// </summary>
        /// <param name="message">Thông điệp lỗi</param>
        /// <returns>ApiResponse với status error</returns>
        public static ApiResponse<object> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = new List<string> { "The requested resource was not found" },
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Class đại diện cho API Response
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của data</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; }
    }
}