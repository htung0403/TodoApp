using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Server.Helper
{
    public static class PasswordHasher
    {
        private const int WorkFactor = 12; // Độ phức tạp của thuật toán BCrypt
        
        /// <summary>
        /// Hash mật khẩu sử dụng BCrypt
        /// </summary>
        /// <param name="password">Mật khẩu cần hash</param>
        /// <returns>Chuỗi hash của mật khẩu</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }
            
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }
        
        /// <summary>
        /// Xác thực mật khẩu với hash đã lưu
        /// </summary>
        /// <param name="password">Mật khẩu cần xác thực</param>
        /// <param name="hashedPassword">Hash đã lưu trong database</param>
        /// <returns>True nếu mật khẩu khớp, False nếu không</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }
            
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // Log exception nếu cần
                return false;
            }
        }
        
        /// <summary>
        /// Kiểm tra xem hash có cần được rehash hay không
        /// (khi workfactor thay đổi)
        /// </summary>
        /// <param name="hashedPassword">Hash hiện tại</param>
        /// <returns>True nếu cần rehash</returns>
        public static bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                return true;
            }
            
            try
            {
                return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
            }
            catch (Exception)
            {
                return true;
            }
        }
        
        /// <summary>
        /// Validate password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>PasswordValidationResult với thông tin validation</returns>
        public static PasswordValidationResult ValidatePasswordStrength(string password)
        {
            var result = new PasswordValidationResult { IsValid = true };
            
            if (string.IsNullOrEmpty(password))
            {
                result.IsValid = false;
                result.Errors.Add("Password cannot be empty");
                return result;
            }
            
            if (password.Length < 6)
            {
                result.IsValid = false;
                result.Errors.Add("Password must be at least 6 characters long");
            }
            
            if (password.Length > 100)
            {
                result.IsValid = false;
                result.Errors.Add("Password cannot be longer than 100 characters");
            }
            
            if (!password.Any(char.IsDigit))
            {
                result.Warnings.Add("Password should contain at least one digit");
            }
            
            if (!password.Any(char.IsUpper))
            {
                result.Warnings.Add("Password should contain at least one uppercase letter");
            }
            
            if (!password.Any(char.IsLower))
            {
                result.Warnings.Add("Password should contain at least one lowercase letter");
            }
            
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                result.Warnings.Add("Password should contain at least one special character");
            }
            
            return result;
        }
    }
    
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        
        public bool HasWarnings => Warnings.Any();
        public string GetErrorsAsString() => string.Join("; ", Errors);
        public string GetWarningsAsString() => string.Join("; ", Warnings);
    }
}