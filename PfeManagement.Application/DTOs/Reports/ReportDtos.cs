using System;

namespace PfeManagement.Application.DTOs.Reports
{
    // A DTO for returning a report. The creation typically uses a generic multipart form request containing the version label and notes.
    public class ReportResponseDto
    {
        public Guid Id { get; set; }
        public int VersionLabel { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
    }
}
