using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;
using PfeManagement.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace PfeManagement.Infrastructure.Services
{
    // GoF Pattern: Adapter (Adapts MailKit's SmtpClient to our INotificationService Target Interface)
    public class EmailNotificationAdapter : INotificationService
    {
        private readonly IConfiguration _config;

        public EmailNotificationAdapter(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string recipient, string subject, string body, bool isHtml = true)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromEmail"]));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            message.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            };

            using var smtpClient = new SmtpClient();
            
            // Connect
            await smtpClient.ConnectAsync(
                emailSettings["Host"], 
                int.Parse(emailSettings["Port"]!), 
                bool.Parse(emailSettings["UseSsl"]!));
                
            // Authenticate
            await smtpClient.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
            
            // Send
            await smtpClient.SendAsync(message);
            
            // Disconnect
            await smtpClient.DisconnectAsync(true);
        }

        public async Task SendVerificationEmailAsync(string email, string name, string token)
        {
            // Similar to existing code logic
            var body = $"Hello {name}, please verify your email using this token: {token}";
            await SendAsync(email, "Verify Your Email", body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string token)
        {
            var body = $"Hello {name}, your reset token is: {token}";
            await SendAsync(email, "Reset Password", body);
        }
    }
}
