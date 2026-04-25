using System.Threading.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Events;

namespace PfeManagement.Application.EventHandlers
{
    public class TaskCreatedNotificationHandler : IDomainEventHandler<TaskCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly PfeManagement.Domain.Interfaces.IUnitOfWork _unitOfWork;

        public TaskCreatedNotificationHandler(INotificationService notificationService, PfeManagement.Domain.Interfaces.IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(TaskCreatedEvent domainEvent)
        {
            if (domainEvent.AssignedToId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(domainEvent.AssignedToId.Value);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var message = $"A new task '{domainEvent.Title}' has been assigned to you.";
                    await _notificationService.SendAsync(user.Email, "New Task Assigned", message, false);
                }
            }
        }
    }
}
