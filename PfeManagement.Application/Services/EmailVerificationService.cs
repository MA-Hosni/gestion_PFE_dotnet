using System;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailVerificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task VerifyEmailAsync(string token)
        {
            var users = await _unitOfWork.Users
                .GetAsync(u => u.VerificationToken == token);
            var user = users.FirstOrDefault()
                ?? throw new Exception("Invalid verification token");

            user.IsVerified        = true;
            user.VerificationToken = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}