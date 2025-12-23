using ClaimsManagement.Business.DTOs.Dashboard;
using ClaimsManagement.Business.DTOs.Claim;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimApprovalRepository _approvalRepository;

        public DashboardService(IClaimRepository claimRepository, IClaimApprovalRepository approvalRepository)
        {
            _claimRepository = claimRepository;
            _approvalRepository = approvalRepository;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId, string userRole)
        {
            var allClaims = userRole == "Employee" 
                ? await _claimRepository.GetByUserIdAsync(userId)
                : await _claimRepository.GetAllAsync();

            var pendingApprovals = userRole == "Manager" || userRole == "Admin"
                ? await _claimRepository.GetPendingApprovalsAsync(userId)
                : new List<DataAccess.Models.Claim>();

            // Calculate today's approvals for managers
            var approvedToday = userRole == "Manager" || userRole == "Admin"
                ? allClaims.Count(c => c.ApprovedAt.HasValue && c.ApprovedAt.Value.Date == DateTime.UtcNow.Date)
                : 0;

            return new DashboardStatsDto
            {
                TotalClaims = allClaims.Count(),
                PendingClaims = allClaims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview),
                SubmittedClaims = allClaims.Count(c => c.Status == ClaimStatus.Submitted),
                UnderReviewClaims = allClaims.Count(c => c.Status == ClaimStatus.UnderReview),
                DraftClaims = allClaims.Count(c => c.Status == ClaimStatus.Draft),
                ApprovedClaims = allClaims.Count(c => c.Status == ClaimStatus.Approved),
                RejectedClaims = allClaims.Count(c => c.Status == ClaimStatus.Rejected),
                PaidClaims = allClaims.Count(c => c.Status == ClaimStatus.Paid),
                TotalAmount = allClaims.Sum(c => c.Amount),
                ApprovedAmount = allClaims.Where(c => c.Status == ClaimStatus.Approved).Sum(c => c.Amount),
                PendingApprovals = pendingApprovals.Count(),
                PendingPayments = allClaims.Count(c => c.Status == ClaimStatus.Approved),
                ApprovedToday = approvedToday,
                OverdueClaims = allClaims.Count(c => c.SLADueDate < DateTime.UtcNow && c.Status != ClaimStatus.Paid && c.Status != ClaimStatus.Rejected)
            };
        }

        public async Task<ClaimAnalyticsDto> GetClaimAnalyticsAsync()
        {
            var allClaims = await _claimRepository.GetAllAsync();

            var analytics = new ClaimAnalyticsDto
            {
                ClaimsByStatus = allClaims.GroupBy(c => c.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                ClaimsByType = allClaims.GroupBy(c => c.ClaimType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                AmountByCategory = allClaims.GroupBy(c => c.Category?.Name ?? "Unknown").ToDictionary(g => g.Key, g => g.Sum(c => c.Amount)),
                ClaimsByMonth = allClaims.GroupBy(c => c.CreatedAt.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count()),
                TopClaimants = allClaims.GroupBy(c => c.User)
                    .Select(g => new TopClaimant
                    {
                        UserName = g.Key != null ? $"{g.Key.FirstName} {g.Key.LastName}" : "Unknown",
                        ClaimCount = g.Count(),
                        TotalAmount = g.Sum(c => c.Amount)
                    })
                    .OrderByDescending(t => t.TotalAmount)
                    .Take(10)
                    .ToList(),
                AverageProcessingTime = allClaims.Where(c => c.ApprovedAt.HasValue)
                    .Average(c => (c.ApprovedAt!.Value - c.SubmittedAt).TotalDays)
            };

            return analytics;
        }

        public async Task<DashboardStatsDto> GetUserStatsAsync(int userId)
        {
            var userClaims = await _claimRepository.GetByUserIdAsync(userId);

            return new DashboardStatsDto
            {
                TotalClaims = userClaims.Count(),
                PendingClaims = userClaims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview),
                SubmittedClaims = userClaims.Count(c => c.Status == ClaimStatus.Submitted),
                UnderReviewClaims = userClaims.Count(c => c.Status == ClaimStatus.UnderReview),
                DraftClaims = userClaims.Count(c => c.Status == ClaimStatus.Draft),
                ApprovedClaims = userClaims.Count(c => c.Status == ClaimStatus.Approved),
                RejectedClaims = userClaims.Count(c => c.Status == ClaimStatus.Rejected),
                PaidClaims = userClaims.Count(c => c.Status == ClaimStatus.Paid),
                TotalAmount = userClaims.Sum(c => c.Amount),
                ApprovedAmount = userClaims.Where(c => c.Status == ClaimStatus.Approved).Sum(c => c.Amount)
            };
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetRecentClaimsAsync(int userId, string userRole)
        {
            var claims = userRole == "Employee"
                ? await _claimRepository.GetByUserIdAsync(userId)
                : await _claimRepository.GetAllAsync();

            var recentClaims = claims.OrderByDescending(c => c.CreatedAt).Take(10);

            return recentClaims.Select(c => new ClaimResponseDto
            {
                ClaimId = c.ClaimId,
                ClaimNumber = c.ClaimNumber,
                ClaimType = c.ClaimType,
                CategoryName = c.Category?.Name ?? "Unknown",
                Title = c.Title,
                Description = c.Description,
                Amount = c.Amount,
                Status = c.Status,
                Priority = c.Priority,
                IncidentDate = c.IncidentDate,
                SubmittedAt = c.SubmittedAt,
                ApprovedAt = c.ApprovedAt,
                PaidAt = c.PaidAt,
                SLADueDate = c.SLADueDate,
                IsEscalated = c.IsEscalated,
                UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                DocumentCount = c.Documents?.Count ?? 0
            });
        }
    }
}