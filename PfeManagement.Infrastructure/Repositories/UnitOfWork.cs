using System.Threading;
using System.Threading.Tasks;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;
using PfeManagement.Infrastructure.Data;

namespace PfeManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<User> Users { get; }
        public IRepository<Student> Students { get; }
        public IRepository<CompanySupervisor> CompanySupervisors { get; }
        public IRepository<UniversitySupervisor> UniversitySupervisors { get; }
        
        public IRepository<Project> Projects { get; }
        public IRepository<Sprint> Sprints { get; }
        public IRepository<UserStory> UserStories { get; }
        public IRepository<TaskItem> Tasks { get; }
        public IRepository<TaskHistory> TaskHistories { get; }
        public IRepository<Report> Reports { get; }
        public IRepository<Meeting> Meetings { get; }
        public IRepository<ValidationRecord> Validations { get; }

        public UnitOfWork(
            AppDbContext context,
            IRepository<User> users,
            IRepository<Student> students,
            IRepository<CompanySupervisor> companySupervisors,
            IRepository<UniversitySupervisor> universitySupervisors,
            IRepository<Project> projects,
            IRepository<Sprint> sprints,
            IRepository<UserStory> userStories,
            IRepository<TaskItem> tasks,
            IRepository<TaskHistory> taskHistories,
            IRepository<Report> reports,
            IRepository<Meeting> meetings,
            IRepository<ValidationRecord> validations)
        {
            _context = context;
            Users = users;
            Students = students;
            CompanySupervisors = companySupervisors;
            UniversitySupervisors = universitySupervisors;
            Projects = projects;
            Sprints = sprints;
            UserStories = userStories;
            Tasks = tasks;
            TaskHistories = taskHistories;
            Reports = reports;
            Meetings = meetings;
            Validations = validations;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
