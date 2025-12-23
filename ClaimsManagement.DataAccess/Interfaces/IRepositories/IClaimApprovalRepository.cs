using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IClaimApprovalRepository : IBaseRepository<ClaimApproval>
    {
        Task<IEnumerable<ClaimApproval>> GetByClaimIdAsync(int claimId);
        Task<IEnumerable<ClaimApproval>> GetByApproverIdAsync(int approverId);
        Task<IEnumerable<ClaimApproval>> GetPendingApprovalsAsync();
        Task<ClaimApproval?> GetByClaimAndLevelAsync(int claimId, int level);
    }
}