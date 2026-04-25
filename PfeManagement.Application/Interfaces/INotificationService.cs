using System.Threading.Tasks;

namespace PfeManagement.Application.Interfaces
{
    // Target interface (what the application needs) for the Adapter Pattern
    public interface INotificationService
    {
        Task SendAsync(string recipient, string subject, string body, bool isHtml = true);
        Task SendVerificationEmailAsync(string email, string name, string token);
        Task SendPasswordResetEmailAsync(string email, string name, string token);
    }
}
