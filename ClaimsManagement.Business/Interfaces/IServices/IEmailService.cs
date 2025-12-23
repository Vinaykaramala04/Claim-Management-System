namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendClaimSubmittedEmailAsync(string userEmail, string claimNumber);
        Task SendClaimApprovedEmailAsync(string userEmail, string claimNumber);
        Task SendClaimRejectedEmailAsync(string userEmail, string claimNumber, string reason);
        Task SendApprovalRequiredEmailAsync(string approverEmail, string claimNumber);
    }
}