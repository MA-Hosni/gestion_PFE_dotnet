using System.Threading.Tasks;
using PfeManagement.Application.DTOs.Auth;

namespace PfeManagement.Application.Interfaces
{
    public interface IPasswordResetService
    {
        Task RequestPasswordResetAsync(PasswordResetRequestDto dto);
        Task ResetPasswordAsync(PasswordResetDto dto);
    }
}