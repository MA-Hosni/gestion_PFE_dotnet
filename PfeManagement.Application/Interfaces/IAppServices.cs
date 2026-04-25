using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.DTOs.Projects;
using PfeManagement.Application.DTOs.Sprints;
using PfeManagement.Application.DTOs.Tasks;
using PfeManagement.Application.DTOs.UserStories;
using PfeManagement.Application.DTOs.Validations;
using PfeManagement.Application.DTOs.Meetings;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegistrationDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task VerifyEmailAsync(string token);
        Task RequestPasswordResetAsync(PasswordResetRequestDto dto);
        Task ResetPasswordAsync(PasswordResetDto dto);
        Task LogoutAsync(string refreshToken);
    }

    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }

    public interface IProjectService
    {
        Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, Guid studentId);
        Task<ProjectResponseDto> GetProjectAsync(Guid projectId);
        Task<ProjectResponseDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto);
        Task DeleteProjectAsync(Guid projectId);
        Task<IEnumerable<ContributorDto>> GetStudentsWithoutProjectAsync();
        Task AddContributorsAsync(Guid projectId, AddRemoveContributorsDto dto, Guid requesterId);
        Task RemoveContributorsAsync(Guid projectId, AddRemoveContributorsDto dto, Guid requesterId);
    }

    public interface ISprintService
    {
        Task<SprintResponseDto> CreateSprintAsync(CreateSprintDto dto, Guid projectId);
        Task<IEnumerable<SprintResponseDto>> GetSprintsAsync(Guid projectId);
        Task<SprintResponseDto> UpdateSprintAsync(Guid sprintId, UpdateSprintDto dto);
        Task DeleteSprintAsync(Guid sprintId);
        Task ReorderSprintsAsync(ReorderSprintsDto dto, Guid projectId);
    }

    public interface IUserStoryService
    {
        Task<UserStoryResponseDto> CreateUserStoryAsync(CreateUserStoryDto dto);
        Task<IEnumerable<UserStoryResponseDto>> GetUserStoriesAsync(Guid sprintId);
        Task<UserStoryResponseDto> UpdateUserStoryAsync(Guid storyId, UpdateUserStoryDto dto);
        Task DeleteUserStoryAsync(Guid storyId);
    }

    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, Guid createdByUserId);
        Task<IEnumerable<TaskResponseDto>> GetTasksAsync(Guid userStoryId);
        Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, Guid modifiedByUserId);
        Task DeleteTaskAsync(Guid taskId);
    }
    public interface IValidationService
    {
        Task<ValidationRecord> CreateValidationAsync(CreateValidationDto dto, Guid validatorId);
    }

    public interface IMeetingService
    {
        Task<Domain.Entities.Meeting> CreateMeetingAsync(CreateMeetingDto dto, Guid creatorId);
    }

    public interface IReportService
    {
        Task<DTOs.Reports.ReportResponseDto> ProcessReportAsync(Guid projectId, string filePath, string notes);
    }

    public interface ITaskReportService
    {
        Task<ProjectTaskReportDto> GetProjectTaskReportAsync(Guid projectId);
        Task<SprintTaskReportDto> GetSprintTaskReportAsync(Guid sprintId);
    }
}
