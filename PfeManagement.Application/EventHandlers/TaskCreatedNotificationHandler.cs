using System.Threading.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Events;

namespace PfeManagement.Application.EventHandlers
{
    // GoF Pattern: Observer - Concrete handler
    public class TaskCreatedNotificationHandler : IDomainEventHandler<TaskCreatedEvent>
    {
        private readonly INotificationService _notificationService;

        public TaskCreatedNotificationHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(TaskCreatedEvent domainEvent)
        {
            try
            {
                // NOTE: We intentionally keep "supervisor@pfemanagement.com" as a placeholder here
                // to match the exact same behavior found in the existing SupervisorNotificationHandler.
                // In a production system, DO NOT use a hardcoded string. Instead, implement a proper 
                // lookup traversing: Task -> UserStory -> Sprint -> Project -> find supervisor email.
                await _notificationService.SendAsync(
                    "supervisor@pfemanagement.com",
                    "New Task Created",
                    $"A new task '{domainEvent.Title}' was created in user story {domainEvent.UserStoryId}.");
            }
            catch
            {
                // Notification delivery failures must not break the core business transaction.
            }
        }
    }
}
