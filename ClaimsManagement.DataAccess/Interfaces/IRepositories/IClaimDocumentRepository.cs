using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IClaimDocumentRepository : IBaseRepository<ClaimDocument>
    {
        Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(int claimId);
        Task<IEnumerable<ClaimDocument>> GetByUserIdAsync(int userId);
    }
}