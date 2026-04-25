using PfeManagement.Domain.Entities;

namespace PfeManagement.Domain.Interfaces
{
    public interface ITaskReportDataAccess
    {
        IRepository<Sprint> Sprints { get; }
        IRepository<UserStory> UserStories { get; }
        IRepository<TaskItem> Tasks { get; }
    }
}
