using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Models.DTOs.Todos;
using Server.Services;
using Server.Helper;
using System.Security.Claims;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        /// <summary>
        /// Get all todos for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTodos()
        {
            try
            {
                var userId = GetCurrentUserId();
                var todos = await _todoService.GetAllTodosAsync(userId);
                
                return Ok(ApiResponseHelper.Success(todos, "Todos retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to retrieve todos", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get a specific todo by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var todo = await _todoService.GetTodoByIdAsync(id, userId);
                
                if (todo == null)
                {
                    return NotFound(ApiResponseHelper.Error("Todo not found", new List<string> { $"Todo with ID {id} not found" }));
                }
                
                return Ok(ApiResponseHelper.Success(todo, "Todo retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to retrieve todo", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Create a new todo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto createTodoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseHelper.Error("Invalid input", errors));
                }

                var userId = GetCurrentUserId();
                var todo = await _todoService.CreateTodoAsync(createTodoDto, userId);
                
                return CreatedAtAction(
                    nameof(GetTodoById), 
                    new { id = todo.Id }, 
                    ApiResponseHelper.Success(todo, "Todo created successfully")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to create todo", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Update an existing todo
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodoDto updateTodoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseHelper.Error("Invalid input", errors));
                }

                var userId = GetCurrentUserId();
                var updatedTodo = await _todoService.UpdateTodoAsync(id, updateTodoDto, userId);
                
                if (updatedTodo == null)
                {
                    return NotFound(ApiResponseHelper.Error("Todo not found", new List<string> { $"Todo with ID {id} not found" }));
                }
                
                return Ok(ApiResponseHelper.Success(updatedTodo, "Todo updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to update todo", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Delete a todo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _todoService.DeleteTodoAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(ApiResponseHelper.Error("Todo not found", new List<string> { $"Todo with ID {id} not found" }));
                }
                
                return Ok(ApiResponseHelper.Success<object>(new object(), "Todo deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to delete todo", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Toggle completion status of a todo
        /// </summary>
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleCompletion(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _todoService.ToggleCompletionAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(ApiResponseHelper.Error("Todo not found", new List<string> { $"Todo with ID {id} not found" }));
                }
                
                // Get the updated todo to return
                var updatedTodo = await _todoService.GetTodoByIdAsync(id, userId);
                
                return Ok(ApiResponseHelper.Success(updatedTodo, "Todo completion status toggled successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Error("Failed to toggle todo completion", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            
            return userId;
        }
    }
}
