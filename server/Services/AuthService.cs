using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models.DTOs.Auth;
using Server.Models.Entities;
using Server.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Server.Helper;

namespace Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;

            // Load JWT settings from configuration
            _jwtSettings = new JwtSettings();
            _configuration.GetSection("Jwt").Bind(_jwtSettings);
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await UserExistsAsync(registerDto.Email))
                {
                    throw new InvalidOperationException("Email is already in use");
                }

                if (await UsernameExistsAsync(registerDto.Username))
                {
                    throw new InvalidOperationException("Username is already in use");
                }

                // Validate password strength
                var passwordValidation = PasswordHasher.ValidatePasswordStrength(registerDto.Password);
                if (!passwordValidation.IsValid)
                {
                    throw new InvalidOperationException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");
                }

                // Hash the password
                var passwordHash = PasswordHasher.HashPassword(registerDto.Password);

                // Create new user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash
                };

                user = await _userRepository.CreateAsync(user);

                // Generate JWT token
                var token = JwtTokenHelper.GenerateJwtToken(user, _jwtSettings);
                var expiresAt = _jwtSettings.GetExpirationDateTime();

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (InvalidOperationException)
            {
                // Rethrow validation exceptions to be handled by controller
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Registration failed: {ex.Message}", ex);
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Verify password
                if (!PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Generate JWT token
                var token = JwtTokenHelper.GenerateJwtToken(user, _jwtSettings);
                var expiresAt = _jwtSettings.GetExpirationDateTime();

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (UnauthorizedAccessException)
            {
                // Rethrow authentication exceptions to be handled by controller
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Login failed: {ex.Message}", ex);
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Error retrieving user");
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return await _userRepository.UsernameExistsAsync(username);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validate user credentials and check if password needs rehash
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="password">Plain text password</param>
        /// <returns>True if valid, False if invalid</returns>
        private async Task<bool> ValidateAndUpdatePasswordIfNeeded(User user, string password)
        {
            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return false;
            }

            // Check if password needs rehashing (when security parameters change)
            if (PasswordHasher.NeedsRehash(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.HashPassword(password);
                await _userRepository.UpdateAsync(user);
            }

            return true;
        }
    }
}
