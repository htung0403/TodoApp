using System.ComponentModel.DataAnnotations;

namespace Server.Models.Entities
{
    public class Todo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Foreign Key
        [Required]
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
