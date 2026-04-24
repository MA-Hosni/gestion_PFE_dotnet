using System;
using System.Collections.Generic;

namespace PfeManagement.WebApi.Models
{
    // ===== Auth DTOs =====

    public class StudentRegistrationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public string Cin { get; set; } = string.Empty;
        public string StudentIdCardIMG { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public Degree Degree { get; set; }
        public DegreeType DegreeType { get; set; }
        public Guid CompSupervisorId { get; set; }
        public Guid UniSupervisorId { get; set; }
    }

    public class CompanySupervisorRegistrationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.CompSupervisor;
        public string CompanyName { get; set; } = string.Empty;
        public string BadgeIMG { get; set; } = string.Empty;
    }

    public class UniversitySupervisorRegistrationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.UniSupervisor;
        public string BadgeIMG { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class PasswordResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordResetDto
    {
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    // ===== Project DTOs =====

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

    // ===== Sprint DTOs =====

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

    // ===== Task DTOs =====

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

    // ===== UserStory DTOs =====

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

    // ===== Validation DTOs =====

    public class CreateValidationDto
    {
        public Guid TaskId { get; set; }
        public TaskItemStatus TaskStatus { get; set; }
        public MeetingType MeetingType { get; set; }
        public Guid? MeetingReferenceId { get; set; }
        public string? Comment { get; set; }
    }

    // ===== Meeting DTOs =====

    public class CreateMeetingDto
    {
        public DateTime ScheduledDate { get; set; }
        public string Agenda { get; set; } = string.Empty;
        public MeetingReferenceType ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }
        public Guid ProjectId { get; set; }
    }

    public class UpdateMeetingDto
    {
        public string? ActualMinutes { get; set; }
    }

    // ===== Report DTOs =====

    public class ReportResponseDto
    {
        public Guid Id { get; set; }
        public int VersionLabel { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
    }
}
