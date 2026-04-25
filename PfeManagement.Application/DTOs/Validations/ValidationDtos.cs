using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.Validations
{
    public class CreateValidationDto
    {
        public Guid TaskId { get; set; }
        public TaskItemStatus TaskStatus { get; set; }
        public MeetingType MeetingType { get; set; }
        public Guid? MeetingReferenceId { get; set; }
        public string? Comment { get; set; }
    }
}
