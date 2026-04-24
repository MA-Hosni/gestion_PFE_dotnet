using System;
using System.Collections.Generic;

namespace PfeManagement.WebApi.Models
{
    // All entities in one file - simple and straightforward

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpires { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class Student : User
    {
        public string Cin { get; set; } = string.Empty;
        public string StudentIdCardIMG { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public Degree Degree { get; set; }
        public DegreeType DegreeType { get; set; }

        public Guid CompSupervisorId { get; set; }
        public virtual CompanySupervisor? CompSupervisor { get; set; }

        public Guid UniSupervisorId { get; set; }
        public virtual UniversitySupervisor? UniSupervisor { get; set; }

        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<Meeting> CreatedMeetings { get; set; } = new List<Meeting>();
    }

    public class CompanySupervisor : User
    {
        public string CompanyName { get; set; } = string.Empty;
        public string BadgeIMG { get; set; } = string.Empty;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public class UniversitySupervisor : User
    {
        public string BadgeIMG { get; set; } = string.Empty;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual ICollection<Student> Contributors { get; set; } = new List<Student>();
        public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }

    public class Sprint
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OrderIndex { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    }

    public class UserStory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public string StoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Medium;
        public int StoryPointEstimate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public Guid SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
        public Priority Priority { get; set; } = Priority.Medium;

        public Guid UserStoryId { get; set; }
        public virtual UserStory? UserStory { get; set; }

        public Guid? AssignedToId { get; set; }
        public virtual User? AssignedTo { get; set; }

        public virtual ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
        public virtual ICollection<ValidationRecord> Validations { get; set; } = new List<ValidationRecord>();
    }

    public class TaskHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public Guid ModifiedById { get; set; }
        public virtual User? ModifiedBy { get; set; }

        public string FieldChanged { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = string.Empty;
    }

    public class Report
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public int VersionLabel { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
    }

    public class Meeting
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public DateTime ScheduledDate { get; set; }
        public string Agenda { get; set; } = string.Empty;
        public string? ActualMinutes { get; set; }

        public MeetingReferenceType ReferenceType { get; set; }
        public Guid ReferenceId { get; set; }

        public Guid CreatedById { get; set; }
        public virtual Student? CreatedBy { get; set; }

        public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;

        public Guid? ValidatorId { get; set; }
        public virtual User? Validator { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
    }

    public class ValidationRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public Guid TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public TaskItemStatus TaskStatus { get; set; }
        public ValidationStatus Status { get; set; } = ValidationStatus.InProgress;

        public Guid ValidatorId { get; set; }
        public virtual User? Validator { get; set; }

        public MeetingType MeetingType { get; set; }

        public Guid? MeetingReferenceId { get; set; }
        public virtual Meeting? MeetingReference { get; set; }

        public string? Comment { get; set; }
    }
}
