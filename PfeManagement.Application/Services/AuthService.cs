using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Factories;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Enums;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly INotificationService _notificationService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly Dictionary<UserRole, UserFactory> _factories;
        private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> RefreshTokens = new();

        public AuthService(
            IUnitOfWork unitOfWork, 
            IPasswordHasher hasher, 
            INotificationService notificationService,
            IJwtTokenService jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _notificationService = notificationService;
            _jwtTokenService = jwtTokenService;
            
            // Map factories manually here for simplicity, in a real app use DI named/typed resolution
            _factories = new Dictionary<UserRole, UserFactory>
            {
                { UserRole.Student, new StudentFactory() },
                { UserRole.CompSupervisor, new CompanySupervisorFactory() },
                { UserRole.UniSupervisor, new UniversitySupervisorFactory() }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegistrationDto dto)
        {
            // Verify email doesn't exist
            var existingUsers = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            if (existingUsers.Count > 0)
                throw new Exception("Email already registered");

            // GoF Factory Method pattern to create correct user subtype
            if (!_factories.TryGetValue(dto.Role, out var factory))
                throw new Exception("Invalid role specified");

            var user = factory.CreateAndRegister(dto, _hasher);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Send Verification Email
            try
            {
                await _notificationService.SendVerificationEmailAsync(user.Email, user.FullName, user.VerificationToken!);
            }
            catch
            {
                // Do not fail registration if email infrastructure is not configured in development.
            }

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();
            if (user == null || !_hasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");
            var refreshDays = 7;
            RefreshTokens[refreshToken] = (user.Id, DateTime.UtcNow.AddDays(refreshDays));

            return BuildAuthResponse(user, accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            if (!RefreshTokens.TryGetValue(dto.RefreshToken, out var tokenData))
            {
                throw new Exception("Invalid refresh token");
            }

            if (tokenData.ExpiresAt <= DateTime.UtcNow)
            {
                RefreshTokens.TryRemove(dto.RefreshToken, out _);
                throw new Exception("Refresh token expired");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(tokenData.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var accessToken = _jwtTokenService.GenerateToken(user);
            return BuildAuthResponse(user, accessToken, dto.RefreshToken);
        }

        public async Task VerifyEmailAsync(string token)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.VerificationToken == token);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                throw new Exception("Invalid verification token");
            }

            user.IsVerified = true;
            user.VerificationToken = null;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RequestPasswordResetAsync(PasswordResetRequestDto dto)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return;
            }

            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _notificationService.SendPasswordResetEmailAsync(user.Email, user.FullName, user.PasswordResetToken);
            }
            catch
            {
                // Ignore email transport errors in development.
            }
        }

        public async Task ResetPasswordAsync(PasswordResetDto dto)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.PasswordResetToken == dto.ResetToken);
            var user = users.FirstOrDefault();
            if (user == null || !user.PasswordResetExpires.HasValue || user.PasswordResetExpires <= DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired reset token");
            }

            user.PasswordHash = _hasher.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpires = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task LogoutAsync(string refreshToken)
        {
            RefreshTokens.TryRemove(refreshToken, out _);
            return Task.CompletedTask;
        }

        private static AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken)
        {
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
