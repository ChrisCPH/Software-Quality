using Microsoft.EntityFrameworkCore;
using RestAPI.Models;

namespace RestAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TaskModel> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>(entity =>
            {
                entity.HasKey(t => t.TaskID);
            });
        }
    }
}