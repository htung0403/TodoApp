using Server.Models.DTOs.Auth;
using System.Threading.Tasks;

namespace Server.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<bool> UserExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }
}
