using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IClaimCommentRepository : IBaseRepository<ClaimComment>
    {
        Task<IEnumerable<ClaimComment>> GetByClaimIdAsync(int claimId);
        Task<IEnumerable<ClaimComment>> GetByUserIdAsync(int userId);
    }
}