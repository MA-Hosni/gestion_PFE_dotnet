using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// Active work: can be completed, blocked, or kept in progress.
    /// </summary>
    public sealed class InProgressState : ITaskState
    {
        public static readonly InProgressState Instance = new();
        private InProgressState() { }

        public TaskItemStatus Key => TaskItemStatus.InProgress;

        public void MoveTo(TaskItem task, TaskItemStatus target)
        {
            switch (target)
            {
                case TaskItemStatus.InProgress:
                    return;
                case TaskItemStatus.Done:
                case TaskItemStatus.Blocked:
                    task.ApplyTransition(target);
                    return;
                case TaskItemStatus.ToDo:
                    TaskStateHelper.Deny(task, target, "Cannot move back to ToDo from InProgress; use explicit domain policy if backlog re-queue is required.");
                    return;
                default:
                    TaskStateHelper.Deny(task, target, "Unsupported target status.");
                    return;
            }
        }
    }
}
