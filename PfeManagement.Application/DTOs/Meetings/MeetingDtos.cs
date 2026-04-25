using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.Meetings
{
    public class CreateMeetingDto
    {
        public DateTime ScheduledDate { get; set; }
        public string Agenda { get; set; } = string.Empty;
        public MeetingReferenceType ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class UpdateMeetingDto
    {
        public string? ActualMinutes { get; set; }
    }
}
