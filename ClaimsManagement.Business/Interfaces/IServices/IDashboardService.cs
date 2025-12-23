using ClaimsManagement.Business.DTOs.Dashboard;
using ClaimsManagement.Business.DTOs.Claim;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string userRole);
        Task<ClaimAnalyticsDto> GetClaimAnalyticsAsync();
        Task<DashboardStatsDto> GetUserStatsAsync(int userId);
        Task<IEnumerable<ClaimResponseDto>> GetRecentClaimsAsync(int userId, string userRole);
    }
}