using System;
using System.Threading.Tasks;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Events;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.EventHandlers
{
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
                FieldChanged = "Creation",
                OldValue = null,
                NewValue = "Task Created",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TaskHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
