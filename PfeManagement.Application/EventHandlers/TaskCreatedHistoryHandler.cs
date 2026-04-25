using System.Threading.Tasks;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Events;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.EventHandlers
{
    // GoF Pattern: Observer - Concrete handler for TaskCreatedEvent
    public class TaskCreatedHistoryHandler : IDomainEventHandler<TaskCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskCreatedHistoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(TaskCreatedEvent domainEvent)
        {
            var history = new TaskHistory
            {
                TaskId = domainEvent.TaskId,
                ModifiedById = domainEvent.CreatedByUserId,
                FieldChanged = "task",
                OldValue = null,
                NewValue = $"Task '{domainEvent.Title}' created"
            };

            await _unitOfWork.TaskHistories.AddAsync(history);
            // SaveChanges is typically handled by the caller of the event dispatcher
        }
    }
}
