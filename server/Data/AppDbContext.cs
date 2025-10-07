using Microsoft.EntityFrameworkCore;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Models.Entities.User> Users { get; set; } = null!;
        public DbSet<Models.Entities.Todo> Todos { get; set; } = null!;
    }
}
