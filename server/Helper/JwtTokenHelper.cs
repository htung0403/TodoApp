using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.Models.Entities;

namespace Server.Helper
{
    public static class JwtTokenHelper
    {
        /// <summary>
        /// Tạo JWT Token cho user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="jwtSettings">JWT settings</param>
        /// <returns>JWT token string</returns>
        public static string GenerateJwtToken(User user, JwtSettings jwtSettings)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            if (jwtSettings == null || !jwtSettings.IsValid())
            {
                throw new ArgumentException("Invalid JWT settings", nameof(jwtSettings));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
            var signingKey = new SymmetricSecurityKey(key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = jwtSettings.GetExpirationDateTime(),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        /// <summary>
        /// Tạo refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
        
        /// <summary>
        /// Validate JWT token và trả về ClaimsPrincipal
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="jwtSettings">JWT settings</param>
        /// <returns>ClaimsPrincipal nếu valid, null nếu invalid</returns>
        public static ClaimsPrincipal? ValidateToken(string token, JwtSettings jwtSettings)
        {
            if (string.IsNullOrEmpty(token) || jwtSettings == null || !jwtSettings.IsValid())
            {
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Lấy user ID từ JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <param name="jwtSettings">JWT settings</param>
        /// <returns>User ID nếu valid, null nếu invalid</returns>
        public static int? GetUserIdFromToken(string token, JwtSettings jwtSettings)
        {
            var principal = ValidateToken(token, jwtSettings);
            if (principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst("userId")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        
        /// <summary>
        /// Kiểm tra xem token có hết hạn hay không
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True nếu token hết hạn</returns>
        public static bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.ValidTo <= DateTime.UtcNow;
            }
            catch (Exception)
            {
                return true;
            }
        }
        
        /// <summary>
        /// Lấy thời gian hết hạn của token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>DateTime hết hạn, null nếu invalid</returns>
        public static DateTime? GetTokenExpirationTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.ValidTo;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}