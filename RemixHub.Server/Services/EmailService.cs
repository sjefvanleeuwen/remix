using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RemixHub.Server.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendVerificationEmailAsync(string to, string userId, string token);
        Task SendPasswordResetEmailAsync(string to, string token);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _baseUrl;
        
        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["Email:SmtpServer"] ?? "";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
            _senderEmail = configuration["Email:SenderEmail"] ?? "";
            _senderName = configuration["Email:SenderName"] ?? "";
            _baseUrl = configuration["AppUrl"] ?? "https://localhost:5001";
        }
        
        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Check if SMTP settings are configured
                if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) || 
                    string.IsNullOrEmpty(_smtpPassword) || string.IsNullOrEmpty(_senderEmail))
                {
                    // Log and return for development without sending emails
                    Console.WriteLine($"Email would be sent to {to} with subject: {subject}");
                    Console.WriteLine($"Body: {body}");
                    return;
                }
                
                var message = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                
                message.To.Add(to);
                
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };
                
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string to, string userId, string token)
        {
            var verificationUrl = $"{_baseUrl}/verify-email?userId={userId}&token={Uri.EscapeDataString(token)}";
            
            var subject = "Verify your RemixHub account";
            var body = $@"
                <h2>Welcome to RemixHub!</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email</a></p>
                <p>If you didn't create this account, you can ignore this email.</p>
            ";
            
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string token)
        {
            var resetUrl = $"{_baseUrl}/reset-password?email={Uri.EscapeDataString(to)}&token={Uri.EscapeDataString(token)}";
            
            var subject = "Reset your RemixHub password";
            var body = $@"
                <h2>Reset Password</h2>
                <p>Please reset your password by clicking the link below:</p>
                <p><a href='{resetUrl}'>Reset Password</a></p>
                <p>If you didn't request a password reset, you can ignore this email.</p>
            ";
            
            await SendEmailAsync(to, subject, body);
        }
    }
}
