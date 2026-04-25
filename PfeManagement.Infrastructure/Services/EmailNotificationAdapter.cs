// ════════════════════════════════════════════════════════════════
//  EmailNotificationAdapter.cs — APRÈS refactoring avec Singleton
//  Couche : PfeManagement.Infrastructure
//  Responsable : Khemissi Nour
//
//  NOTE : Ce fichier applique AUSSI le patron GoF Adapter
//         (adapte MailKit SmtpClient vers INotificationService).
//         Le Singleton est utilisé pour la configuration email.
//
//  AVANT : _config.GetSection("EmailSettings") relu à chaque envoi
//  APRÈS : AppConfigurationManager.Instance — valeurs en mémoire
// ════════════════════════════════════════════════════════════════

using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using PfeManagement.Application.Interfaces;

namespace PfeManagement.Infrastructure.Services
{
    // GoF Adapter : adapte MailKit SmtpClient vers INotificationService
    public class EmailNotificationAdapter : INotificationService
    {
        // ── Référence vers le Singleton ──────────────────────────
        // Même instance que JwtTokenService — un seul objet en RAM.
        private readonly AppConfigurationManager _config;

        public EmailNotificationAdapter()
        {
            _config = AppConfigurationManager.Instance;
        }

        /// <summary>
        /// Envoie un email via MailKit (SmtpClient).
        /// Les paramètres SMTP viennent du Singleton (déjà en RAM).
        /// </summary>
        public async Task SendAsync(
            string recipient,
            string subject,
            string body,
            bool isHtml = true)
        {
            var message = new MimeMessage();

            // Paramètres lus depuis le Singleton — pas de GetSection()
            message.From.Add(new MailboxAddress(
                _config.EmailFromName,
                _config.EmailFromEmail));

            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;
            message.Body    = new TextPart(
                isHtml ? TextFormat.Html : TextFormat.Plain)
            { Text = body };

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _config.EmailHost,
                _config.EmailPort,
                _config.EmailUseSsl);

            await smtpClient.AuthenticateAsync(
                _config.EmailUsername,
                _config.EmailPassword);

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }

        public async Task SendVerificationEmailAsync(
            string email, string name, string token)
        {
            var body = $"Bonjour {name},<br/>" +
                       $"Veuillez vérifier votre compte avec ce token : " +
                       $"<strong>{token}</strong>";
            await SendAsync(email,
                "Vérification de votre compte — PFE Management",
                body);
        }

        public async Task SendPasswordResetEmailAsync(
            string email, string name, string token)
        {
            var body = $"Bonjour {name},<br/>" +
                       $"Votre token de réinitialisation : " +
                       $"<strong>{token}</strong>";
            await SendAsync(email,
                "Réinitialisation de mot de passe — PFE Management",
                body);
        }
    }
}