using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.Tasks
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; }
        public Priority Priority { get; set; }
        public Guid UserStoryId { get; set; }
        public Guid? AssignedToId { get; set; }
    }

    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskItemStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public Guid? AssignedToId { get; set; }
    }

    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; }
        public Priority Priority { get; set; }
        public Guid UserStoryId { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}
