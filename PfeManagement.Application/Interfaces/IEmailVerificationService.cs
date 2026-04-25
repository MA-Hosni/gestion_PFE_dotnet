using System.Threading.Tasks;

namespace PfeManagement.Application.Interfaces
{
    public interface IEmailVerificationService
    {
        Task VerifyEmailAsync(string token);
    }
}