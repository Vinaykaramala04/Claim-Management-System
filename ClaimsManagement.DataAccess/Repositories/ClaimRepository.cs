using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class ClaimRepository : BaseRepository<Claim>, IClaimRepository
    {
        public ClaimRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<Claim?> GetByClaimNumberAsync(string claimNumber)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                .Include(c => c.Approvals)
                .Include(c => c.Comments)
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
        }

        public async Task<IEnumerable<Claim>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.Category)
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetByStatusAsync(ClaimStatus status)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingApprovalsAsync(int approverId)
        {
            // Return claims that are UnderReview and need manager approval
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Include(c => c.StatusHistory)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Where(c => c.Status == ClaimStatus.UnderReview)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetEscalatedClaimsAsync()
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Where(c => c.IsEscalated)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetClaimsBySLAAsync(DateTime slaDate)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Where(c => c.SLADueDate <= slaDate && c.Status != ClaimStatus.Paid && c.Status != ClaimStatus.Rejected)
                .OrderBy(c => c.SLADueDate)
                .ToListAsync();
        }

        public override async Task<Claim?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Category)
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Include(c => c.Approvals)
                    .ThenInclude(a => a.Approver)
                .Include(c => c.Comments)
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
        }
    }
}