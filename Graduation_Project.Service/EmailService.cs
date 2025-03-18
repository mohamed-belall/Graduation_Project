using Graduation_Project.Core.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;
namespace Graduation_Project.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Read Email settings from configuration
                string smtpServer = _configuration["EmailSettings:SMTPServer"] ?? throw new ArgumentNullException("SMTPServer");
                int smtpPort = int.Parse(_configuration["EmailSettings:SMTPPort"] ?? throw new ArgumentNullException("SMTPPort"));
                string smtpUsername = _configuration["EmailSettings:SMTPUsername"] ?? throw new ArgumentNullException("SMTPUsername");
                string smtpPassword = _configuration["EmailSettings:SMTPPassword"] ?? throw new ArgumentNullException("SMTPPassword");
                bool useTLS = bool.Parse(_configuration["EmailSettings:UseTLS"] ?? throw new ArgumentNullException("UseTLS"));
                string senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail");
                string senderName = _configuration["EmailSettings:SenderName"] ?? throw new ArgumentNullException("SenderName");

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpServer, smtpPort, useTLS ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
    }
}
