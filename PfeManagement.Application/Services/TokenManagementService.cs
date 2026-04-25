using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Entities;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.Application.Services
{
    public class TokenManagementService : ITokenManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;

        private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)>
            _refreshTokens = new();

        public TokenManagementService(
            IUnitOfWork unitOfWork,
            IJwtTokenService jwtTokenService)
        {
            _unitOfWork      = unitOfWork;
            _jwtTokenService = jwtTokenService;
        }

        public (string AccessToken, string RefreshToken) GenerateTokenPair(User user)
        {
            var accessToken  = _jwtTokenService.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");
            _refreshTokens[refreshToken] = (user.Id, DateTime.UtcNow.AddDays(7));
            return (accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            if (!_refreshTokens.TryGetValue(dto.RefreshToken, out var tokenData))
                throw new Exception("Invalid refresh token");

            if (tokenData.ExpiresAt <= DateTime.UtcNow)
            {
                _refreshTokens.TryRemove(dto.RefreshToken, out _);
                throw new Exception("Refresh token expired");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(tokenData.UserId)
                ?? throw new Exception("User not found");

            var accessToken = _jwtTokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId       = user.Id,
                Email        = user.Email,
                Role         = user.Role,
                IsVerified   = user.IsVerified,
                AccessToken  = accessToken,
                RefreshToken = dto.RefreshToken
            };
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _refreshTokens.TryRemove(refreshToken, out _);
        }
    }
}