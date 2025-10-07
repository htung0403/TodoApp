using Server.Models.DTOs.Todos;
using Server.Models.Entities;
using Server.Repositories;

namespace Server.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;

        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<IEnumerable<TodoDto>> GetAllTodosAsync(int userId)
        {
            var todos = await _todoRepository.GetByUserIdAsync(userId);

            return todos.Select(t => new TodoDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt
            });
        }

        public async Task<TodoDto?> GetTodoByIdAsync(int id, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            
            if (todo == null || todo.UserId != userId)
                return null;

            return new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                DueDate = todo.DueDate,
                CompletedAt = todo.CompletedAt
            };
        }

        public async Task<TodoDto> CreateTodoAsync(CreateTodoDto createTodoDto, int userId)
        {
            var todo = new Todo
            {
                Title = createTodoDto.Title,
                Description = createTodoDto.Description,
                UserId = userId,
                IsCompleted = false,
                DueDate = createTodoDto.DueDate
            };

            var createdTodo = await _todoRepository.CreateAsync(todo);

            return new TodoDto
            {
                Id = createdTodo.Id,
                Title = createdTodo.Title,
                Description = createdTodo.Description,
                IsCompleted = createdTodo.IsCompleted,
                CreatedAt = createdTodo.CreatedAt,
                UpdatedAt = createdTodo.UpdatedAt,
                DueDate = createdTodo.DueDate,
                CompletedAt = createdTodo.CompletedAt
            };
        }

        public async Task<TodoDto?> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            
            if (todo == null || todo.UserId != userId)
                return null;

            // Update properties
            if (updateTodoDto.Title != null)
                todo.Title = updateTodoDto.Title;
            
            if (updateTodoDto.Description != null)
                todo.Description = updateTodoDto.Description;
            
            if (updateTodoDto.DueDate.HasValue)
                todo.DueDate = updateTodoDto.DueDate;

            // Handle IsCompleted update
            if (updateTodoDto.IsCompleted.HasValue)
            {
                var newCompletedStatus = updateTodoDto.IsCompleted.Value;
                
                // If completing the todo, set CompletedAt
                if (!todo.IsCompleted && newCompletedStatus)
                {
                    todo.CompletedAt = DateTime.UtcNow;
                }
                // If uncompleting the todo, clear CompletedAt
                else if (todo.IsCompleted && !newCompletedStatus)
                {
                    todo.CompletedAt = null;
                }

                todo.IsCompleted = newCompletedStatus;
            }

            var updatedTodo = await _todoRepository.UpdateAsync(todo);

            return new TodoDto
            {
                Id = updatedTodo.Id,
                Title = updatedTodo.Title,
                Description = updatedTodo.Description,
                IsCompleted = updatedTodo.IsCompleted,
                CreatedAt = updatedTodo.CreatedAt,
                UpdatedAt = updatedTodo.UpdatedAt,
                DueDate = updatedTodo.DueDate,
                CompletedAt = updatedTodo.CompletedAt
            };
        }

        public async Task<bool> DeleteTodoAsync(int id, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            
            if (todo == null || todo.UserId != userId)
                return false;

            return await _todoRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleCompletionAsync(int id, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            
            if (todo == null || todo.UserId != userId)
                return false;

            // Toggle completion status
            todo.IsCompleted = !todo.IsCompleted;

            // Set or clear completion timestamp
            if (todo.IsCompleted)
            {
                todo.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                todo.CompletedAt = null;
            }

            await _todoRepository.UpdateAsync(todo);
            return true;
        }
    }
}