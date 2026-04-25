using System;
using PfeManagement.Domain.Common;
using PfeManagement.Domain.Enums;

namespace PfeManagement.Domain.Entities
{
    public abstract class User : SoftDeletableEntity
    {
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
        
        public void GenerateVerificationToken()
        {
            VerificationToken = Guid.NewGuid().ToString("N");
        }
    }
}
