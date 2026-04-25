using System;
using PfeManagement.Domain.Common;

namespace PfeManagement.Domain.Entities
{
    public class TaskHistory : BaseEntity
    {
        public Guid TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public Guid ModifiedById { get; set; }
        public virtual User? ModifiedBy { get; set; }

        public string FieldChanged { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = string.Empty;
    }
}
