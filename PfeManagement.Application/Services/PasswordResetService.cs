using System;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly INotificationService _notificationService;

        public PasswordResetService(
            IUnitOfWork unitOfWork,
            IPasswordHasher hasher,
            INotificationService notificationService)
        {
            _unitOfWork          = unitOfWork;
            _hasher              = hasher;
            _notificationService = notificationService;
        }

        public async Task RequestPasswordResetAsync(PasswordResetRequestDto dto)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            var user  = users.FirstOrDefault();
            if (user == null) return;

            user.PasswordResetToken   = Guid.NewGuid().ToString("N");
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _notificationService.SendPasswordResetEmailAsync(
                    user.Email, user.FullName, user.PasswordResetToken);
            }
            catch { }
        }

        public async Task ResetPasswordAsync(PasswordResetDto dto)
        {
            var users = await _unitOfWork.Users
                .GetAsync(u => u.PasswordResetToken == dto.ResetToken);
            var user = users.FirstOrDefault();

            if (user == null
                || !user.PasswordResetExpires.HasValue
                || user.PasswordResetExpires <= DateTime.UtcNow)
                throw new Exception("Invalid or expired reset token");

            user.PasswordHash         = _hasher.HashPassword(dto.NewPassword);
            user.PasswordResetToken   = null;
            user.PasswordResetExpires = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}