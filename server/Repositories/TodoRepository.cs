using Server.Data;
using Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Server.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _context;

        public TodoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Todo?> GetByIdAsync(int id)
        {
            return await _context.Todos
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Todo>> GetAllAsync()
        {
            return await _context.Todos
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Todo>> GetByUserIdAsync(int userId)
        {
            return await _context.Todos
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueDate.HasValue ? t.DueDate : DateTime.MaxValue)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Todo> CreateAsync(Todo todo)
        {
            todo.CreatedAt = DateTime.UtcNow;
            
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            
            return todo;
        }

        public async Task<Todo> UpdateAsync(Todo todo)
        {
            todo.UpdatedAt = DateTime.UtcNow;
            
            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();
            
            return todo;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return false;

            _context.Todos.Remove(todo);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Todos.AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Todo>> GetOverdueTodosAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.Todos
                .Where(t => t.UserId == userId && 
                           !t.IsCompleted && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value.Date < today)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Todo>> GetTodosDueTodayAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.Todos
                .Where(t => t.UserId == userId && 
                           !t.IsCompleted && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value.Date == today)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Todo>> GetCompletedTodosAsync(int userId)
        {
            return await _context.Todos
                .Where(t => t.UserId == userId && t.IsCompleted)
                .OrderByDescending(t => t.CompletedAt ?? t.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Todo>> GetActiveTodosAsync(int userId)
        {
            return await _context.Todos
                .Where(t => t.UserId == userId && !t.IsCompleted)
                .OrderBy(t => t.DueDate.HasValue ? t.DueDate : DateTime.MaxValue)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTodoCountByUserAsync(int userId)
        {
            return await _context.Todos
                .CountAsync(t => t.UserId == userId);
        }

        public async Task<int> GetCompletedTodoCountByUserAsync(int userId)
        {
            return await _context.Todos
                .CountAsync(t => t.UserId == userId && t.IsCompleted);
        }
    }
}