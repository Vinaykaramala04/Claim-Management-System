using ClaimsManagement.Business.DTOs.ClaimApproval;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IClaimApprovalService
    {
        Task<ApprovalResponseDto> ProcessApprovalAsync(int claimId, ApprovalStatus status, int approverId, string? comments = null);
        Task<IEnumerable<ApprovalResponseDto>> GetPendingApprovalsAsync(int approverId);
        Task<IEnumerable<ApprovalResponseDto>> GetApprovalsByClaimIdAsync(int claimId);
        Task<ApprovalResponseDto?> GetApprovalByIdAsync(int approvalId);
    }
}