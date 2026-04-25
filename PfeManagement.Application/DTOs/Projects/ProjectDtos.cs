using System;
using System.Collections.Generic;

namespace PfeManagement.Application.DTOs.Projects
{
    public class CreateProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> Contributors { get; set; } = new List<Guid>();
    }

    public class UpdateProjectDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AddRemoveContributorsDto
    {
        public List<Guid> StudentIds { get; set; } = new List<Guid>();
    }

    public class ProjectResponseDto
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ContributorDto> Contributors { get; set; } = new List<ContributorDto>();
    }

    public class ContributorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
