using System.Threading.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Events;

namespace PfeManagement.Application.EventHandlers
{
    // GoF Pattern: Observer - Concrete handler
    public class SupervisorNotificationHandler : IDomainEventHandler<TaskStatusChangedEvent>
    {
        private readonly INotificationService _notificationService;

        public SupervisorNotificationHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(TaskStatusChangedEvent domainEvent)
        {
            if (domainEvent.NewStatus == TaskItemStatus.Done)
            {
                try
                {
                    // In a real app, query the task -> user story -> sprint -> project -> supervisor emails
                    // For simplicity here, we simulate sending a notification.
                    await _notificationService.SendAsync(
                        "supervisor@pfemanagement.com",
                        "Task Completed",
                        $"Task {domainEvent.TaskId} was marked as Done.");
                }
                catch
                {
                    // Notification delivery failures must not break the core business transaction.
                }
            }
        }
    }
}
