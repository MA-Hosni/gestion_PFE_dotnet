using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;

namespace PfeManagement.Application.Interfaces
{
    public interface IUserRegistrationService
    {
        Task<AuthResponseDto> RegisterAsync(UserRegistrationDto dto);
    }
}