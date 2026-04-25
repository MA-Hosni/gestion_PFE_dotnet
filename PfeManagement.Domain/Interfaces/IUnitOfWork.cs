using System;
using System.Threading;
using System.Threading.Tasks;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Student> Students { get; }
        IRepository<CompanySupervisor> CompanySupervisors { get; }
        IRepository<UniversitySupervisor> UniversitySupervisors { get; }
        
        IRepository<Project> Projects { get; }
        IRepository<Sprint> Sprints { get; }
        IRepository<UserStory> UserStories { get; }
        IRepository<TaskItem> Tasks { get; }
        IRepository<TaskHistory> TaskHistories { get; }
        IRepository<Report> Reports { get; }
        IRepository<Meeting> Meetings { get; }
        IRepository<ValidationRecord> Validations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
