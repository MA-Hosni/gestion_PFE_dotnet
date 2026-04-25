using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Events
{
    public class TaskStatusChangedEvent : IDomainEvent
    {
        public Guid TaskId { get; }
        public TaskItemStatus OldStatus { get; }
        public TaskItemStatus NewStatus { get; }
        public Guid ChangedByUserId { get; }

        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public TaskStatusChangedEvent(Guid taskId, TaskItemStatus oldStatus, TaskItemStatus newStatus, Guid changedByUserId)
        {
            TaskId = taskId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedByUserId = changedByUserId;
        }
    }
}
