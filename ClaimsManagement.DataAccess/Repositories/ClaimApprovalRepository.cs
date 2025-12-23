using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class ClaimApprovalRepository : BaseRepository<ClaimApproval>, IClaimApprovalRepository
    {
        public ClaimApprovalRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClaimApproval>> GetByClaimIdAsync(int claimId)
        {
            return await _dbSet
                .Include(ca => ca.Approver)
                .Where(ca => ca.ClaimId == claimId)
                .OrderBy(ca => ca.ApprovalLevel)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClaimApproval>> GetByApproverIdAsync(int approverId)
        {
            return await _dbSet
                .Include(ca => ca.Claim)
                .ThenInclude(c => c.User)
                .Where(ca => ca.ApproverId == approverId)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClaimApproval>> GetPendingApprovalsAsync()
        {
            return await _dbSet
                .Include(ca => ca.Claim)
                .ThenInclude(c => c.User)
                .Include(ca => ca.Approver)
                .Where(ca => ca.Status == ApprovalStatus.Pending)
                .OrderByDescending(ca => ca.CreatedAt)
                .ToListAsync();
        }

        public async Task<ClaimApproval?> GetByClaimAndLevelAsync(int claimId, int level)
        {
            return await _dbSet
                .Include(ca => ca.Approver)
                .FirstOrDefaultAsync(ca => ca.ClaimId == claimId && ca.ApprovalLevel == level);
        }
    }
}