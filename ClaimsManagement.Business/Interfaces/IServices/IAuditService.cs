namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IAuditService
    {
        Task LogClaimCreatedAsync(int claimId, int userId, string details);
        Task LogStatusChangeAsync(int claimId, int userId, string oldStatus, string newStatus, string? comments = null);
        Task LogDocumentUploadAsync(int claimId, int userId, string fileName);
        Task LogApprovalActionAsync(int claimId, int approverId, string action, string? comments = null);
    }
}