using System;
using System.Collections.Generic;

namespace PfeManagement.Application.DTOs.Sprints
{
    public class CreateSprintDto
    {
        public string Title { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UpdateSprintDto
    {
        public string? Title { get; set; }
        public string? Goal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ReorderSprintsDto
    {
        public List<SprintOrderDto> Sprints { get; set; } = new List<SprintOrderDto>();
    }

    public class SprintOrderDto
    {
        public Guid SprintId { get; set; }
        public int OrderIndex { get; set; }
    }

    public class SprintResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OrderIndex { get; set; }
    }
}
