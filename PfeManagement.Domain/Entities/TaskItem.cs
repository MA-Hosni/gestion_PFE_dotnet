using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Exceptions;
using PfeManagement.Domain.TaskStates;

namespace PfeManagement.Domain.Entities
{
    /// <summary>
    /// Context for the State pattern: status is persisted as <see cref="TaskItemStatus"/> for EF Core;
    /// behaviour is delegated to <see cref="ITaskState"/> implementations.
    /// </summary>
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>Persisted status (EF Core column). Mutated only via <see cref="ApplyTransition"/>.</summary>
        public TaskItemStatus Status { get; private set; } = TaskItemStatus.ToDo;

        public Priority Priority { get; set; } = Priority.Medium;

        public Guid UserStoryId { get; set; }
        public virtual UserStory? UserStory { get; set; }

        public Guid? AssignedToId { get; set; }
        public virtual User? AssignedTo { get; set; }

        public virtual ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
        public virtual ICollection<ValidationRecord> Validations { get; set; } = new List<ValidationRecord>();

        /// <summary>Resolves the current state object (flyweight).</summary>
        public ITaskState GetState() => TaskStateRegistry.Resolve(Status);

        /// <summary>
        /// Generic transition: dispatches to the current state's <see cref="ITaskState.MoveTo"/>.
        /// </summary>
        public void MoveTo(TaskItemStatus target)
        {
            if (target == Status)
                return;

            TaskStateRegistry.Resolve(Status).MoveTo(this, target);
        }

        /// <summary>Explicit lifecycle: ToDo -> InProgress only.</summary>
        public void StartTask()
        {
            if (Status != TaskItemStatus.ToDo)
            {
                throw new InvalidTaskStateTransitionException(
                    Status,
                    TaskItemStatus.InProgress,
                    "StartTask is only valid when the task is in ToDo. Use ReopenTask after a block.");
            }

            MoveTo(TaskItemStatus.InProgress);
        }

        /// <summary>Explicit lifecycle: InProgress -> Done only.</summary>
        public void CompleteTask()
        {
            if (Status != TaskItemStatus.InProgress)
            {
                throw new InvalidTaskStateTransitionException(
                    Status,
                    TaskItemStatus.Done,
                    "CompleteTask is only valid when the task is InProgress.");
            }

            MoveTo(TaskItemStatus.Done);
        }

        /// <summary>Explicit lifecycle: ToDo or InProgress -> Blocked.</summary>
        public void BlockTask()
        {
            if (Status != TaskItemStatus.ToDo && Status != TaskItemStatus.InProgress)
            {
                throw new InvalidTaskStateTransitionException(
                    Status,
                    TaskItemStatus.Blocked,
                    "BlockTask is only valid when the task is ToDo or InProgress.");
            }

            MoveTo(TaskItemStatus.Blocked);
        }

        /// <summary>Explicit lifecycle: Blocked -> InProgress (resume after unblock).</summary>
        public void ReopenTask()
        {
            if (Status != TaskItemStatus.Blocked)
            {
                throw new InvalidTaskStateTransitionException(
                    Status,
                    TaskItemStatus.InProgress,
                    "ReopenTask is only valid when the task is Blocked.");
            }

            MoveTo(TaskItemStatus.InProgress);
        }

        /// <summary>
        /// Internal hook used by state objects and by the aggregate when first creating a task.
        /// </summary>
        internal void ApplyTransition(TaskItemStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
