using Server.Models.Entities;

namespace Server.Repositories
{
    public interface ITodoRepository
    {
        Task<Todo?> GetByIdAsync(int id);
        Task<IEnumerable<Todo>> GetAllAsync();
        Task<IEnumerable<Todo>> GetByUserIdAsync(int userId);
        Task<Todo> CreateAsync(Todo todo);
        Task<Todo> UpdateAsync(Todo todo);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Todo>> GetOverdueTodosAsync(int userId);
        Task<IEnumerable<Todo>> GetTodosDueTodayAsync(int userId);
        Task<IEnumerable<Todo>> GetCompletedTodosAsync(int userId);
        Task<IEnumerable<Todo>> GetActiveTodosAsync(int userId);
        Task<int> GetTodoCountByUserAsync(int userId);
        Task<int> GetCompletedTodoCountByUserAsync(int userId);
    }
}