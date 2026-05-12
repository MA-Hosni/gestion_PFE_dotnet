using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// Terminal state: task is locked; no further workflow transitions.
    /// </summary>
    public sealed class DoneState : ITaskState
    {
        public static readonly DoneState Instance = new();
        private DoneState() { }

        public TaskItemStatus Key => TaskItemStatus.Done;

        public void MoveTo(TaskItem task, TaskItemStatus target)
        {
            if (target == TaskItemStatus.Done)
                return;

            TaskStateHelper.Deny(task, target, "Completed tasks are locked; reopening finished work is not allowed by the current state machine.");
        }
    }
}
