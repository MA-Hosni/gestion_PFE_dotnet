using System;
using System.Collections.Generic;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
        public Priority Priority { get; set; } = Priority.Medium;

        public Guid UserStoryId { get; set; }
        public virtual UserStory? UserStory { get; set; }

        public Guid? AssignedToId { get; set; }
        public virtual User? AssignedTo { get; set; }

        public virtual ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
        public virtual ICollection<ValidationRecord> Validations { get; set; } = new List<ValidationRecord>();
    }
}
