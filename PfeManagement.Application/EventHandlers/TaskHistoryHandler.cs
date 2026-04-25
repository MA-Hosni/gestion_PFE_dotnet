using System.Threading.Tasks;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Events;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.EventHandlers
{
    // GoF Pattern: Observer - Concrete handler for TaskStatusChangedEvent
    public class TaskHistoryHandler : IDomainEventHandler<TaskStatusChangedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskHistoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(TaskStatusChangedEvent domainEvent)
        {
            var history = new TaskHistory
            {
                TaskId = domainEvent.TaskId,
                ModifiedById = domainEvent.ChangedByUserId,
                FieldChanged = "status",
                OldValue = domainEvent.OldStatus.ToString(),
                NewValue = domainEvent.NewStatus.ToString()
            };

            await _unitOfWork.TaskHistories.AddAsync(history);
            // SaveChanges is typically handled by the caller of the event dispatcher
        }
    }
}
