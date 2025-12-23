using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class ClaimDocumentRepository : BaseRepository<ClaimDocument>, IClaimDocumentRepository
    {
        public ClaimDocumentRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(int claimId)
        {
            return await _dbSet
                .Include(cd => cd.UploadedByUser)
                .Where(cd => cd.ClaimId == claimId)
                .OrderByDescending(cd => cd.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClaimDocument>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(cd => cd.Claim)
                .Where(cd => cd.UploadedBy == userId)
                .OrderByDescending(cd => cd.UploadedAt)
                .ToListAsync();
        }
    }
}