using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IClaimRepository _claimRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(IDashboardService dashboardService, IClaimRepository claimRepository, IUserRepository userRepository)
        {
            _dashboardService = dashboardService;
            _claimRepository = claimRepository;
            _userRepository = userRepository;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var stats = await _dashboardService.GetDashboardStatsAsync(userId, userRole);
            return Ok(stats);
        }

        [HttpGet("analytics")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetClaimAnalytics()
        {
            try
            {
                var allClaims = await _claimRepository.GetAllAsync();
                var allUsers = await _userRepository.GetAllAsync();

                var totalClaims = allClaims.Count();
                var approvedClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);
                var pendingClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Submitted || c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.UnderReview);
                var rejectedClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Rejected);

                var analytics = new
                {
                    totalClaims = totalClaims,
                    approvedClaims = approvedClaims,
                    pendingClaims = pendingClaims,
                    rejectedClaims = rejectedClaims,
                    totalAmount = allClaims.Sum(c => c.Amount),
                    approvedAmount = allClaims.Where(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved).Sum(c => c.Amount),
                    totalUsers = allUsers.Count(),
                    claimsByCategory = allClaims.GroupBy(c => c.Category?.Name ?? "Unknown")
                        .Select(g => new {
                            category = g.Key,
                            count = g.Count(),
                            percentage = totalClaims > 0 ? Math.Round((double)g.Count() / totalClaims * 100, 1) : 0
                        }).ToList(),
                    claimsByStatus = new[] {
                        new { status = "Approved", count = approvedClaims, percentage = totalClaims > 0 ? Math.Round((double)approvedClaims / totalClaims * 100, 1) : 0 },
                        new { status = "Pending", count = pendingClaims, percentage = totalClaims > 0 ? Math.Round((double)pendingClaims / totalClaims * 100, 1) : 0 },
                        new { status = "Rejected", count = rejectedClaims, percentage = totalClaims > 0 ? Math.Round((double)rejectedClaims / totalClaims * 100, 1) : 0 }
                    },
                    monthlyTrends = allClaims.GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                        .Select(g => new {
                            month = g.Key,
                            claims = g.Count(),
                            amount = g.Sum(c => c.Amount)
                        }).OrderBy(x => x.month).ToList(),
                    topUsers = allClaims.GroupBy(c => c.User)
                        .Where(g => g.Key != null)
                        .Select(g => new {
                            userName = $"{g.Key!.FirstName} {g.Key.LastName}",
                            claimCount = g.Count(),
                            totalAmount = g.Sum(c => c.Amount)
                        })
                        .OrderByDescending(x => x.totalAmount)
                        .Take(5)
                        .ToList()
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("analytics/team/stats")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTeamStats()
        {
            try
            {
                var allClaims = await _claimRepository.GetAllAsync();
                var allUsers = await _userRepository.GetAllAsync();

                var totalClaims = allClaims.Count();
                var approvedClaims = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);
                var totalProcessingDays = allClaims.Where(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved && c.UpdatedAt.HasValue)
                    .Sum(c => (c.UpdatedAt.Value - c.CreatedAt).TotalDays);
                var approvedClaimsCount = allClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);

                var teamStats = new
                {
                    totalTeamMembers = allUsers.Count(u => u.Role == ClaimsManagement.DataAccess.Enum.UserRole.Employee),
                    totalClaims = totalClaims,
                    avgApprovalRate = totalClaims > 0 ? Math.Round((double)approvedClaims / totalClaims * 100, 1) : 0,
                    avgProcessingTime = approvedClaimsCount > 0 ? Math.Round(totalProcessingDays / approvedClaimsCount, 1) : 0
                };

                return Ok(teamStats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("analytics/team/members")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTeamMembers()
        {
            try
            {
                var allClaims = await _claimRepository.GetAllAsync();
                var employees = await _userRepository.GetAllAsync();
                var employeeUsers = employees.Where(u => u.Role == ClaimsManagement.DataAccess.Enum.UserRole.Employee).ToList();

                var teamMembers = employeeUsers.Select(user => {
                    var userClaims = allClaims.Where(c => c.UserId == user.UserId).ToList();
                    var approvedClaims = userClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);
                    var pendingClaims = userClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Submitted || 
                                                              c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.UnderReview);
                    var totalProcessingDays = userClaims.Where(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved && c.UpdatedAt.HasValue)
                        .Sum(c => (c.UpdatedAt.Value - c.CreatedAt).TotalDays);
                    var approvedCount = userClaims.Count(c => c.Status == ClaimsManagement.DataAccess.Enum.ClaimStatus.Approved);

                    return new {
                        id = user.UserId,
                        name = $"{user.FirstName} {user.LastName}",
                        totalClaims = userClaims.Count,
                        approvedClaims = approvedClaims,
                        pendingClaims = pendingClaims,
                        totalAmount = userClaims.Sum(c => c.Amount),
                        avgProcessingTime = approvedCount > 0 ? Math.Round(totalProcessingDays / approvedCount, 1) : 0
                    };
                }).ToList();

                return Ok(teamMembers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user-stats/{userId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            var stats = await _dashboardService.GetUserStatsAsync(userId);
            return Ok(stats);
        }

        [HttpGet("recent-claims")]
        public async Task<IActionResult> GetRecentClaims()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var claims = await _dashboardService.GetRecentClaimsAsync(userId, userRole);
            return Ok(claims);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "Employee";
        }
    }
}