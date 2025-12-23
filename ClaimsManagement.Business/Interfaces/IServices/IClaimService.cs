using ClaimsManagement.Business.DTOs.Claim;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IClaimService
    {
        Task<ClaimResponseDto> CreateClaimAsync(ClaimCreateDto request, int userId);
        Task<ClaimResponseDto?> GetClaimByIdAsync(int claimId);
        Task<ClaimResponseDto?> GetClaimByNumberAsync(string claimNumber);
        Task<IEnumerable<ClaimResponseDto>> GetClaimsByUserAsync(int userId);
        Task<IEnumerable<ClaimResponseDto>> GetClaimsByStatusAsync(ClaimStatus status);
        Task<ClaimResponseDto> UpdateClaimStatusAsync(int claimId, ClaimStatus status, int updatedBy, string? comments = null);
        Task<IEnumerable<ClaimResponseDto>> GetPendingApprovalsAsync(int approverId);
        Task<string> GenerateClaimNumberAsync();
    }
}