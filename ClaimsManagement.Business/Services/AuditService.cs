using ClaimsManagement.Business.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public async Task LogClaimCreatedAsync(int claimId, int userId, string details)
        {
            try
            {
                _logger.LogInformation("AUDIT: Claim {ClaimId} created by user {UserId}. Details: {Details}", 
                    claimId, userId, details);
                
                // TODO: Store in audit table
                // var auditLog = new AuditLog
                // {
                //     EntityType = "Claim",
                //     EntityId = claimId,
                //     Action = "Created",
                //     UserId = userId,
                //     Details = details,
                //     Timestamp = DateTime.UtcNow
                // };
                // await _auditRepository.AddAsync(auditLog);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log claim creation audit for claim {ClaimId}", claimId);
            }
        }

        public async Task LogStatusChangeAsync(int claimId, int userId, string oldStatus, string newStatus, string? comments = null)
        {
            try
            {
                var details = $"Status changed from {oldStatus} to {newStatus}";
                if (!string.IsNullOrEmpty(comments))
                    details += $". Comments: {comments}";

                _logger.LogInformation("AUDIT: Claim {ClaimId} status changed by user {UserId}. {Details}", 
                    claimId, userId, details);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log status change audit for claim {ClaimId}", claimId);
            }
        }

        public async Task LogDocumentUploadAsync(int claimId, int userId, string fileName)
        {
            try
            {
                _logger.LogInformation("AUDIT: Document {FileName} uploaded to claim {ClaimId} by user {UserId}", 
                    fileName, claimId, userId);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document upload audit for claim {ClaimId}", claimId);
            }
        }

        public async Task LogApprovalActionAsync(int claimId, int approverId, string action, string? comments = null)
        {
            try
            {
                var details = $"Approval action: {action}";
                if (!string.IsNullOrEmpty(comments))
                    details += $". Comments: {comments}";

                _logger.LogInformation("AUDIT: Claim {ClaimId} approval action by user {ApproverId}. {Details}", 
                    claimId, approverId, details);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log approval action audit for claim {ClaimId}", claimId);
            }
        }
    }
}