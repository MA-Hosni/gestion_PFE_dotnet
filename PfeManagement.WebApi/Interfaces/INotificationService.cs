using System.Threading.Tasks;

namespace PfeManagement.WebApi.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(string recipient, string subject, string body, bool isHtml = true);
        Task SendVerificationEmailAsync(string email, string name, string token);
        Task SendPasswordResetEmailAsync(string email, string name, string token);
    }
}
