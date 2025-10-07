using Server.Models.DTOs.Todos;

namespace Server.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoDto>> GetAllTodosAsync(int userId);
        Task<TodoDto?> GetTodoByIdAsync(int id, int userId);
        Task<TodoDto> CreateTodoAsync(CreateTodoDto createTodoDto, int userId);
        Task<TodoDto?> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, int userId);
        Task<bool> DeleteTodoAsync(int id, int userId);
        Task<bool> ToggleCompletionAsync(int id, int userId);
    }
}
