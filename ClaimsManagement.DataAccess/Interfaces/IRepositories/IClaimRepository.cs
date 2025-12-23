using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IClaimRepository : IBaseRepository<Claim>
    {
        Task<Claim?> GetByClaimNumberAsync(string claimNumber);
        Task<IEnumerable<Claim>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Claim>> GetByStatusAsync(ClaimStatus status);
        Task<IEnumerable<Claim>> GetPendingApprovalsAsync(int approverId);
        Task<IEnumerable<Claim>> GetEscalatedClaimsAsync();
        Task<IEnumerable<Claim>> GetClaimsBySLAAsync(DateTime slaDate);
    }
}