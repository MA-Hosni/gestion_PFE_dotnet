using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Exceptions;

namespace PfeManagement.Domain.TaskStates
{
    internal static class TaskStateHelper
    {
        internal static void Deny(TaskItem task, TaskItemStatus target, string reason)
        {
            throw new InvalidTaskStateTransitionException(task.Status, target, reason);
        }
    }
}
