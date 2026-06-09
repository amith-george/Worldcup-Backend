using Microsoft.EntityFrameworkCore;
using WorldCupPolling.Models;

namespace WorldCupPolling.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce the constraint: One User can only have One Vote
            modelBuilder.Entity<Vote>()
                .HasIndex(v => v.UserId)
                .IsUnique();

            // Seed default configuration settings if they don't exist
            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { Key = "AreResultsRevealed", Value = "false" }
            );
        }
    }
}