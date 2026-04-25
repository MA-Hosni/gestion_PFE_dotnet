using System;
using System.Collections.Generic;

namespace PfeManagement.Application.DTOs.Tasks
{
    public class ProjectTaskReportDto
    {
        public Guid ProjectId { get; set; }
        public int TotalTasks { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class SprintTaskReportDto
    {
        public Guid SprintId { get; set; }
        public int TotalTasks { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();
    }
}
