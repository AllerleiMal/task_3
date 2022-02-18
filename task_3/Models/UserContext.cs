using Microsoft.EntityFrameworkCore;

namespace task_3.Models
{
    public class UserContext : DbContext
    {
            public DbSet<User> Users { get; set; }
            public UserContext(DbContextOptions<UserContext> options) : base(options)
            {
                Database.EnsureCreated();
            }
    }
}