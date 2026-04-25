using Microsoft.EntityFrameworkCore;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<CompanySupervisor> CompanySupervisors => Set<CompanySupervisor>();
        public DbSet<UniversitySupervisor> UniversitySupervisors => Set<UniversitySupervisor>();
        
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Sprint> Sprints => Set<Sprint>();
        public DbSet<UserStory> UserStories => Set<UserStory>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<ValidationRecord> Validations => Set<ValidationRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all IEntityTypeConfiguration from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
