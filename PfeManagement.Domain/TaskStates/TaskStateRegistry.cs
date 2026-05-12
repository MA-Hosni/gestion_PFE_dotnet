using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.TaskStates
{
    /// <summary>
    /// Resolves the state object for a persisted status value (states are flyweights).
    /// </summary>
    public static class TaskStateRegistry
    {
        public static ITaskState Resolve(TaskItemStatus status)
        {
            return status switch
            {
                TaskItemStatus.ToDo => ToDoState.Instance,
                TaskItemStatus.InProgress => InProgressState.Instance,
                TaskItemStatus.Blocked => BlockedState.Instance,
                TaskItemStatus.Done => DoneState.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown task status")
            };
        }
    }
}
