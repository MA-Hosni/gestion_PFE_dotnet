using System;
using System.Linq;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenManagementService _tokenService;

        public UserLoginService(
            IUnitOfWork unitOfWork,
            IPasswordHasher hasher,
            ITokenManagementService tokenService)
        {
            _unitOfWork   = unitOfWork;
            _hasher       = hasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var users = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);
            var user  = users.FirstOrDefault();

            if (user == null || !_hasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(user);

            return new AuthResponseDto
            {
                UserId       = user.Id,
                Email        = user.Email,
                Role         = user.Role,
                IsVerified   = user.IsVerified,
                AccessToken  = accessToken,
                RefreshToken = refreshToken
            };
        }

        public Task LogoutAsync(string refreshToken)
        {
            _tokenService.RevokeRefreshToken(refreshToken);
            return Task.CompletedTask;
        }
    }
}