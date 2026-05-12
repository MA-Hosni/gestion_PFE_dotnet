using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Exceptions
{
    /// <summary>
    /// Raised when a <see cref="Entities.TaskItem"/> transition violates the State machine rules.
    /// </summary>
    public class InvalidTaskStateTransitionException : DomainConstraintException
    {
        public TaskItemStatus Current { get; }
        public TaskItemStatus Requested { get; }

        public InvalidTaskStateTransitionException(TaskItemStatus current, TaskItemStatus requested, string message)
            : base(message)
        {
            Current = current;
            Requested = requested;
        }
    }
}
