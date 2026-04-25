using System;
using System.Threading;
using System.Threading.Tasks;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Domain.Interfaces
{
    public interface IUnitOfWork : ITaskReportDataAccess, IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Student> Students { get; }
        IRepository<CompanySupervisor> CompanySupervisors { get; }
        IRepository<UniversitySupervisor> UniversitySupervisors { get; }
        
        IRepository<Project> Projects { get; }
        new IRepository<Sprint> Sprints { get; }
        new IRepository<UserStory> UserStories { get; }
        new IRepository<TaskItem> Tasks { get; }
        IRepository<TaskHistory> TaskHistories { get; }
        IRepository<Report> Reports { get; }
        IRepository<Meeting> Meetings { get; }
        IRepository<ValidationRecord> Validations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
