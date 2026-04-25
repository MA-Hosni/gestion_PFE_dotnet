using System;

namespace PfeManagement.Domain.Events
{
    public class TaskCreatedEvent : IDomainEvent
    {
        public Guid TaskId { get; }
        public string Title { get; }
        public Guid? AssignedToId { get; }
        public Guid CreatedByUserId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public TaskCreatedEvent(Guid taskId, string title, Guid? assignedToId, Guid createdByUserId)
        {
            TaskId = taskId;
            Title = title;
            AssignedToId = assignedToId;
            CreatedByUserId = createdByUserId;
        }
    }
}
