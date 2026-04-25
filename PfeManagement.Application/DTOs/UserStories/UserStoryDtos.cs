using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.UserStories
{
    public class CreateUserStoryDto
    {
        public string StoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public int StoryPointEstimate { get; set; }
        public DateTime DueDate { get; set; }
        public Guid SprintId { get; set; }
    }

    public class UpdateUserStoryDto
    {
        public string? StoryName { get; set; }
        public string? Description { get; set; }
        public Priority? Priority { get; set; }
        public int? StoryPointEstimate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UserStoryResponseDto
    {
        public Guid Id { get; set; }
        public string StoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public int StoryPointEstimate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public Guid SprintId { get; set; }
    }
}
