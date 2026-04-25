using System;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public class Meeting : SoftDeletableEntity
    {
        public DateTime ScheduledDate { get; set; }
        public string Agenda { get; set; } = string.Empty;
        public string? ActualMinutes { get; set; }

        public MeetingReferenceType ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }

        public Guid CreatedById { get; set; }
        public virtual Student? CreatedBy { get; set; }

        public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;

        public Guid? ValidatorId { get; set; }
        public virtual User? Validator { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
    }
}
