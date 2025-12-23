using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class ClaimCommentRepository : BaseRepository<ClaimComment>, IClaimCommentRepository
    {
        public ClaimCommentRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClaimComment>> GetByClaimIdAsync(int claimId)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.ClaimId == claimId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClaimComment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(cc => cc.Claim)
                .Where(cc => cc.UserId == userId)
                .OrderByDescending(cc => cc.CreatedAt)
                .ToListAsync();
        }
    }
}