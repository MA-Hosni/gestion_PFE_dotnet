using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// Task is in backlog: can be started, blocked, but not completed without work in progress.
    /// </summary>
    public sealed class ToDoState : ITaskState
    {
        public static readonly ToDoState Instance = new();
        private ToDoState() { }

        public TaskItemStatus Key => TaskItemStatus.ToDo;

        public void MoveTo(TaskItem task, TaskItemStatus target)
        {
            switch (target)
            {
                case TaskItemStatus.ToDo:
                    return;
                case TaskItemStatus.InProgress:
                case TaskItemStatus.Blocked:
                    task.ApplyTransition(target);
                    return;
                case TaskItemStatus.Done:
                    TaskStateHelper.Deny(task, target, "Cannot complete a task directly from ToDo; start work first (ToDo -> InProgress -> Done).");
                    return;
                default:
                    TaskStateHelper.Deny(task, target, "Unsupported target status.");
                    return;
            }
        }
    }
}
