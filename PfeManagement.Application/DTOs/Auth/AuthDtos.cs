using System;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Application.DTOs.Auth
{
    public abstract class UserRegistrationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class StudentRegistrationDto : UserRegistrationDto
    {
        public string Cin { get; set; } = string.Empty;
        public string StudentIdCardIMG { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public Degree Degree { get; set; }
        public DegreeType DegreeType { get; set; }
        public Guid CompSupervisorId { get; set; }
        public Guid UniSupervisorId { get; set; }

        public StudentRegistrationDto()
        {
            Role = UserRole.Student;
        }
    }

    public class CompanySupervisorRegistrationDto : UserRegistrationDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string BadgeIMG { get; set; } = string.Empty;

        public CompanySupervisorRegistrationDto()
        {
            Role = UserRole.CompSupervisor;
        }
    }

    public class UniversitySupervisorRegistrationDto : UserRegistrationDto
    {
        public string BadgeIMG { get; set; } = string.Empty;

        public UniversitySupervisorRegistrationDto()
        {
            Role = UserRole.UniSupervisor;
        }
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
}
