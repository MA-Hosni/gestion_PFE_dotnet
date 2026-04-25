using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Factories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly INotificationService _notificationService;
        private readonly Dictionary<UserRole, UserFactory> _factories;

        public UserRegistrationService(
            IUnitOfWork unitOfWork,
            IPasswordHasher hasher,
            INotificationService notificationService)
        {
            _unitOfWork          = unitOfWork;
            _hasher              = hasher;
            _notificationService = notificationService;

            _factories = new Dictionary<UserRole, UserFactory>
            {
                { UserRole.Student,        new StudentFactory() },
                { UserRole.CompSupervisor, new CompanySupervisorFactory() },
                { UserRole.UniSupervisor,  new UniversitySupervisorFactory() }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegistrationDto dto)
        {
            var existing = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            if (existing.Count > 0)
                throw new Exception("Email already registered");

            if (!_factories.TryGetValue(dto.Role, out var factory))
                throw new Exception("Invalid role specified");

            var user = factory.CreateAndRegister(dto, _hasher);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _notificationService.SendVerificationEmailAsync(
                    user.Email, user.FullName, user.VerificationToken!);
            }
            catch { }

            return new AuthResponseDto
            {
                UserId     = user.Id,
                Email      = user.Email,
                Role       = user.Role,
                IsVerified = user.IsVerified
            };
        }
    }
}