using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace PfeManagement.WebApi.Helpers
{
    // Simple static helper for sending emails - no interface/adapter pattern needed
    public static class EmailHelper
    {
        public static async Task SendEmail(string recipient, string subject, string body, IConfiguration config, bool isHtml = true)
        {
            var emailSettings = config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromEmail"]));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            message.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            };

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                emailSettings["Host"],
                int.Parse(emailSettings["Port"]!),
                bool.Parse(emailSettings["UseSsl"]!));
            await smtpClient.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }

        public static async Task SendVerificationEmail(string email, string name, string token, IConfiguration config)
        {
            var body = $"Hello {name}, please verify your email using this token: {token}";
            await SendEmail(email, "Verify Your Email", body, config);
        }

        public static async Task SendPasswordResetEmail(string email, string name, string token, IConfiguration config)
        {
            var body = $"Hello {name}, your reset token is: {token}";
            await SendEmail(email, "Reset Password", body, config);
        }
    }
}
