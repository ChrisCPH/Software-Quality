using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskModel> TaskModel { get; set; }
        public DbSet<List> Lists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>()
                .HasKey(t => t.TaskID);

            modelBuilder.Entity<TaskModel>()
                .Property(t => t.Title)
                .IsRequired();

            modelBuilder.Entity<List>()
                .HasKey(l => l.ListID);
        }
    }
}

