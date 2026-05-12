using System;
using System.Collections.Generic;
using PfeManagement.Application.DTOs.Sprints;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.DTOs.UserStories;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.Facades
{
    /// <summary>
    /// Request: create a sprint then attach zero or more user stories in one orchestrated workflow.
    /// </summary>
    public class CreateSprintWithUserStoriesDto
    {
        public CreateSprintDto Sprint { get; set; } = new();

        /// <summary>Stories to create on the new sprint; <see cref="CreateUserStoryDto.SprintId"/> is set by the facade.</summary>
        public List<CreateUserStoryForSprintItemDto> UserStories { get; set; } = new();
    }

    /// <summary>
    /// User story fields for batch creation (no sprint id — supplied after sprint exists).
    /// </summary>
    public class CreateUserStoryForSprintItemDto
    {
        public string StoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public int StoryPointEstimate { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class SprintPlanningWorkflowResultDto
    {
        public SprintResponseDto Sprint { get; set; } = new();
        public IReadOnlyList<UserStoryResponseDto> UserStories { get; set; } = Array.Empty<UserStoryResponseDto>();
    }

    /// <summary>
    /// Request: create one user story then seed initial tasks (all ToDo) in one workflow.
    /// </summary>
    public class CreateUserStoryWithTasksDto
    {
        public CreateUserStoryDto UserStory { get; set; } = new();
        public List<InitialTaskItemDto> InitialTasks { get; set; } = new();
    }

    public class InitialTaskItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public Guid? AssignedToId { get; set; }
    }

    public class UserStoryWithTasksWorkflowResultDto
    {
        public UserStoryResponseDto UserStory { get; set; } = new();
        public IReadOnlyList<TaskResponseDto> Tasks { get; set; } = Array.Empty<TaskResponseDto>();
    }

    /// <summary>Lightweight projection for cross-sprint listing within a project.</summary>
    public class UserStoryPlanningSummaryDto
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

    /// <summary>
    /// Aggregated read model: sprints plus project-wide task distribution (facade combines two subsystems).
    /// </summary>
    public class ProjectPlanningOverviewDto
    {
        public IReadOnlyList<SprintResponseDto> Sprints { get; set; } = Array.Empty<SprintResponseDto>();
        public ProjectTaskReportDto TaskReport { get; set; } = new();
    }
}
