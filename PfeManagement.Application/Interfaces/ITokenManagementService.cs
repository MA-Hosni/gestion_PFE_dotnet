using System;
using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Domain.Entities;

namespace PfeManagement.Application.Interfaces
{
    public interface ITokenManagementService
    {
        (string AccessToken, string RefreshToken) GenerateTokenPair(User user);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        void RevokeRefreshToken(string refreshToken);
    }
}