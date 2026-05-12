using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// Task is blocked / waiting: cannot jump to Done without resuming work first.
    /// </summary>
    public sealed class BlockedState : ITaskState
    {
        public static readonly BlockedState Instance = new();
        private BlockedState() { }

        public TaskItemStatus Key => TaskItemStatus.Blocked;

        public void MoveTo(TaskItem task, TaskItemStatus target)
        {
            switch (target)
            {
                case TaskItemStatus.Blocked:
                    return;
                case TaskItemStatus.InProgress:
                case TaskItemStatus.ToDo:
                    task.ApplyTransition(target);
                    return;
                case TaskItemStatus.Done:
                    TaskStateHelper.Deny(task, target, "Cannot complete a blocked task without resuming (Blocked -> InProgress or ToDo, then Done).");
                    return;
                default:
                    TaskStateHelper.Deny(task, target, "Unsupported target status.");
                    return;
            }
        }
    }
}
