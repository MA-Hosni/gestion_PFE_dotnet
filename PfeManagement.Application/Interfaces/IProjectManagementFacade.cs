using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Facades;

namespace PfeManagement.Application.Interfaces
{
    /// <summary>
    /// Structural Facade (GoF): simplifies multi-step project / sprint / story / task workflows
    /// by orchestrating existing application services behind a single entry point.
    /// </summary>
    public interface IProjectManagementFacade
    {
        /// <summary>Creates a sprint then creates related user stories bound to that sprint.</summary>
        Task<SprintPlanningWorkflowResultDto> CreateSprintWithUserStoriesAsync(
            Guid projectId,
            CreateSprintWithUserStoriesDto dto);

        /// <summary>Creates a user story then creates initial tasks (ToDo) for that story.</summary>
        Task<UserStoryWithTasksWorkflowResultDto> CreateUserStoryWithInitialTasksAsync(
            CreateUserStoryWithTasksDto dto,
            Guid createdByUserId);

        /// <summary>Lists all user stories belonging to any sprint of the given project.</summary>
        Task<IReadOnlyList<UserStoryPlanningSummaryDto>> GetUserStoriesForProjectAsync(Guid projectId);

        /// <summary>Combines sprint listing with project task report for dashboard / planning views.</summary>
        Task<ProjectPlanningOverviewDto> GetProjectPlanningOverviewAsync(Guid projectId);
    }
}
