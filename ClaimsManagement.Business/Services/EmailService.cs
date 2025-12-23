using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging as needed)
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendClaimSubmittedEmailAsync(string userEmail, string claimNumber)
        {
            var subject = "Claim Submitted Successfully";
            var body = $@"
                <h2>Claim Submitted</h2>
                <p>Your claim <strong>{claimNumber}</strong> has been submitted successfully.</p>
                <p>You will receive updates as your claim progresses through the approval process.</p>
                <p>Thank you for using our Claims Management System.</p>";

            await SendEmailAsync(userEmail, subject, body);
        }

        public async Task SendClaimApprovedEmailAsync(string userEmail, string claimNumber)
        {
            var subject = "Claim Approved";
            var body = $@"
                <h2>Claim Approved</h2>
                <p>Great news! Your claim <strong>{claimNumber}</strong> has been approved.</p>
                <p>Payment processing will begin shortly.</p>
                <p>Thank you for using our Claims Management System.</p>";

            await SendEmailAsync(userEmail, subject, body);
        }

        public async Task SendClaimRejectedEmailAsync(string userEmail, string claimNumber, string reason)
        {
            var subject = "Claim Rejected";
            var body = $@"
                <h2>Claim Rejected</h2>
                <p>We regret to inform you that your claim <strong>{claimNumber}</strong> has been rejected.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>If you have questions, please contact our support team.</p>
                <p>Thank you for using our Claims Management System.</p>";

            await SendEmailAsync(userEmail, subject, body);
        }

        public async Task SendApprovalRequiredEmailAsync(string approverEmail, string claimNumber)
        {
            var subject = "Claim Approval Required";
            var body = $@"
                <h2>Approval Required</h2>
                <p>A new claim <strong>{claimNumber}</strong> requires your approval.</p>
                <p>Please log in to the Claims Management System to review and process this claim.</p>
                <p>Thank you.</p>";

            await SendEmailAsync(approverEmail, subject, body);
        }
    }
}