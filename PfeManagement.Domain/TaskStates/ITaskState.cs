using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// State Pattern: encapsulates behaviour that depends on the current <see cref="TaskItemStatus"/>.
    /// Each concrete state decides which transitions are legal and applies domain rules.
    /// </summary>
    public interface ITaskState
    {
        TaskItemStatus Key { get; }

        /// <summary>
        /// Performs a transition to <paramref name="target"/> if allowed; otherwise throws.
        /// </summary>
        void MoveTo(TaskItem task, TaskItemStatus target);
    }
}
