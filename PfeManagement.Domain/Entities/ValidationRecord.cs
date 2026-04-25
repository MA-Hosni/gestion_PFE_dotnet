using System;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public class ValidationRecord : SoftDeletableEntity
    {
        public Guid TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public TaskItemStatus TaskStatus { get; set; }
        public ValidationStatus Status { get; set; } = ValidationStatus.InProgress;

        public Guid ValidatorId { get; set; }
        public virtual User? Validator { get; set; }

        public MeetingType MeetingType { get; set; }

        public Guid? MeetingReferenceId { get; set; }
        public virtual Meeting? MeetingReference { get; set; }

        public string? Comment { get; set; }
    }
}
