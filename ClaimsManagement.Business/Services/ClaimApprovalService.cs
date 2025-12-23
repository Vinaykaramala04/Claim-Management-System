using ClaimsManagement.Business.DTOs.ClaimApproval;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class ClaimApprovalService : IClaimApprovalService
    {
        private readonly IClaimApprovalRepository _approvalRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ClaimApprovalService> _logger;

        public ClaimApprovalService(
            IClaimApprovalRepository approvalRepository, 
            IClaimRepository claimRepository,
            INotificationService notificationService,
            ILogger<ClaimApprovalService> logger)
        {
            _approvalRepository = approvalRepository;
            _claimRepository = claimRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ApprovalResponseDto> ProcessApprovalAsync(int claimId, ApprovalStatus status, int approverId, string? comments = null)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null)
                throw new ArgumentException("Claim not found");

            var approval = new ClaimApproval
            {
                ClaimId = claimId,
                ApproverId = approverId,
                Status = status,
                ApprovalLevel = 1,
                Comments = comments,
                ApprovedAt = status == ApprovalStatus.Approved ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };

            var savedApproval = await _approvalRepository.AddAsync(approval);

            // Update claim status based on approval
            if (status == ApprovalStatus.Approved)
            {
                claim.Status = ClaimStatus.Approved;
                claim.ApprovedAt = DateTime.UtcNow;
                
                // Notify claim owner about approval
                await _notificationService.CreateNotificationAsync(
                    claim.UserId,
                    "Claim Approved",
                    $"Great news! Your claim {claim.ClaimNumber} for ${claim.Amount:F2} has been approved and is ready for payment processing.",
                    NotificationType.ClaimApproved,
                    claim.ClaimId
                );
            }
            else if (status == ApprovalStatus.Rejected)
            {
                claim.Status = ClaimStatus.Rejected;
                
                // Notify claim owner about rejection
                await _notificationService.CreateNotificationAsync(
                    claim.UserId,
                    "Claim Rejected",
                    $"Your claim {claim.ClaimNumber} has been rejected. Reason: {comments ?? "No reason provided"}",
                    NotificationType.ClaimRejected,
                    claim.ClaimId
                );
            }

            await _claimRepository.UpdateAsync(claim);
            
            _logger.LogInformation("Claim {ClaimId} approval processed with status {Status} by approver {ApproverId}", claimId, status, approverId);
            return MapToResponseDto(savedApproval);
        }

        public async Task<IEnumerable<ApprovalResponseDto>> GetPendingApprovalsAsync(int approverId)
        {
            var approvals = await _approvalRepository.GetByApproverIdAsync(approverId);
            return approvals.Where(a => a.Status == ApprovalStatus.Pending).Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ApprovalResponseDto>> GetApprovalsByClaimIdAsync(int claimId)
        {
            var approvals = await _approvalRepository.GetByClaimIdAsync(claimId);
            return approvals.Select(MapToResponseDto);
        }

        public async Task<ApprovalResponseDto?> GetApprovalByIdAsync(int approvalId)
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalId);
            return approval != null ? MapToResponseDto(approval) : null;
        }

        private ApprovalResponseDto MapToResponseDto(ClaimApproval approval)
        {
            return new ApprovalResponseDto
            {
                ApprovalId = approval.ApprovalId,
                ClaimId = approval.ClaimId,
                ClaimNumber = approval.Claim?.ClaimNumber ?? "Unknown",
                ApproverName = approval.Approver != null ? $"{approval.Approver.FirstName} {approval.Approver.LastName}" : "Unknown",
                Status = approval.Status,
                ApprovalLevel = approval.ApprovalLevel,
                Comments = approval.Comments,
                ApprovedAt = approval.ApprovedAt,
                CreatedAt = approval.CreatedAt
            };
        }
    }
}