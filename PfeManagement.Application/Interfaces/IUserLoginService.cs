using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;

namespace PfeManagement.Application.Interfaces
{
    public interface IUserLoginService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task LogoutAsync(string refreshToken);
    }
}