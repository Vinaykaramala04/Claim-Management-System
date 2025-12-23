using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IEmailNotificationService
    {
        Task SendClaimStatusUpdateAsync(Claim claim, ClaimStatus oldStatus, ClaimStatus newStatus);
        Task SendSLAWarningAsync(Claim claim);
        Task SendApprovalRequestAsync(Claim claim, int approverId);
        Task SendClaimCreatedAsync(Claim claim);
    }
}