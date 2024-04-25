using Microsoft.EntityFrameworkCore;


namespace UserManagementApi.Server.Data
{
    public class UserManagementContext : DbContext 
    {
        public UserManagementContext(DbContextOptions<UserManagementContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
