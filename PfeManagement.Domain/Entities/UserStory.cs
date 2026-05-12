using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public class UserStory : SoftDeletableEntity
    {
        public string StoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Medium;
        public int StoryPointEstimate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public Guid SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// GRASP Creator: UserStory aggregates TaskItem instances for this story.
        /// </summary>
        public TaskItem AddTask(
            string title,
            string description,
            TaskItemStatus status,
            Priority priority,
            Guid? assignedToId)
        {
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                Status = status,
                Priority = priority,
                UserStoryId = Id,
                UserStory = this,
                AssignedToId = assignedToId
            };

            Tasks.Add(task);
            return task;
        }
    }
}
