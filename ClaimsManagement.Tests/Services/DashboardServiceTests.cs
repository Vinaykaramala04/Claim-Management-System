using Xunit;
using Moq;
using FluentAssertions;
using ClaimsManagement.Business.Services;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Tests.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<IClaimRepository> _mockClaimRepository;
        private readonly Mock<IClaimApprovalRepository> _mockApprovalRepository;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _mockClaimRepository = new Mock<IClaimRepository>();
            _mockApprovalRepository = new Mock<IClaimApprovalRepository>();
            _dashboardService = new DashboardService(_mockClaimRepository.Object, _mockApprovalRepository.Object);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_EmployeeRole_ShouldReturnUserSpecificStats()
        {
            // Arrange
            var userId = 1;
            var userRole = "Employee";
            var userClaims = new List<Claim>
            {
                new Claim { ClaimId = 1, UserId = userId, Status = ClaimStatus.Submitted, Amount = 100.00m },
                new Claim { ClaimId = 2, UserId = userId, Status = ClaimStatus.Approved, Amount = 200.00m },
                new Claim { ClaimId = 3, UserId = userId, Status = ClaimStatus.Paid, Amount = 150.00m }
            };

            _mockClaimRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(userClaims);

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync(userId, userRole);

            // Assert
            result.Should().NotBeNull();
            result.TotalClaims.Should().Be(3);
            result.SubmittedClaims.Should().Be(1);
            result.ApprovedClaims.Should().Be(1);
            result.PaidClaims.Should().Be(1);
            result.TotalAmount.Should().Be(450.00m);
            result.ApprovedAmount.Should().Be(200.00m);
            result.PendingClaims.Should().Be(1); // Only submitted claims are pending for employees
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ManagerRole_ShouldReturnAllClaimsStats()
        {
            // Arrange
            var userId = 2;
            var userRole = "Manager";
            var allClaims = new List<Claim>
            {
                new Claim { ClaimId = 1, UserId = 1, Status = ClaimStatus.Submitted, Amount = 100.00m },
                new Claim { ClaimId = 2, UserId = 1, Status = ClaimStatus.UnderReview, Amount = 200.00m },
                new Claim { ClaimId = 3, UserId = 3, Status = ClaimStatus.Approved, Amount = 150.00m, ApprovedAt = DateTime.UtcNow },
                new Claim { ClaimId = 4, UserId = 3, Status = ClaimStatus.Rejected, Amount = 75.00m }
            };

            var pendingApprovals = new List<Claim>
            {
                allClaims[0], // Submitted claim
                allClaims[1]  // Under review claim
            };

            _mockClaimRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allClaims);
            _mockClaimRepository.Setup(x => x.GetPendingApprovalsAsync(userId))
                .ReturnsAsync(pendingApprovals);

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync(userId, userRole);

            // Assert
            result.Should().NotBeNull();
            result.TotalClaims.Should().Be(4);
            result.SubmittedClaims.Should().Be(1);
            result.UnderReviewClaims.Should().Be(1);
            result.ApprovedClaims.Should().Be(1);
            result.RejectedClaims.Should().Be(1);
            result.PendingClaims.Should().Be(2); // Submitted + Under Review
            result.PendingApprovals.Should().Be(2);
            result.TotalAmount.Should().Be(525.00m);
            result.ApprovedAmount.Should().Be(150.00m);
            result.ApprovedToday.Should().Be(1);
        }

        [Fact]
        public async Task GetClaimAnalyticsAsync_ShouldReturnComprehensiveAnalytics()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim 
                { 
                    ClaimId = 1, 
                    Status = ClaimStatus.Submitted, 
                    ClaimType = ClaimType.Medical, 
                    Amount = 100.00m,
                    CreatedAt = new DateTime(2024, 1, 15),
                    SubmittedAt = new DateTime(2024, 1, 15),
                    ApprovedAt = new DateTime(2024, 1, 20),
                    Category = new ExpenseCategory { Name = "Medical" },
                    User = new User { FirstName = "John", LastName = "Doe" }
                },
                new Claim 
                { 
                    ClaimId = 2, 
                    Status = ClaimStatus.Approved, 
                    ClaimType = ClaimType.Travel, 
                    Amount = 200.00m,
                    CreatedAt = new DateTime(2024, 2, 10),
                    SubmittedAt = new DateTime(2024, 2, 10),
                    ApprovedAt = new DateTime(2024, 2, 12),
                    Category = new ExpenseCategory { Name = "Travel" },
                    User = new User { FirstName = "Jane", LastName = "Smith" }
                },
                new Claim 
                { 
                    ClaimId = 3, 
                    Status = ClaimStatus.Paid, 
                    ClaimType = ClaimType.Medical, 
                    Amount = 150.00m,
                    CreatedAt = new DateTime(2024, 1, 25),
                    SubmittedAt = new DateTime(2024, 1, 25),
                    ApprovedAt = new DateTime(2024, 1, 28),
                    Category = new ExpenseCategory { Name = "Medical" },
                    User = new User { FirstName = "John", LastName = "Doe" }
                }
            };

            _mockClaimRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(claims);

            // Act
            var result = await _dashboardService.GetClaimAnalyticsAsync();

            // Assert
            result.Should().NotBeNull();
            
            // Test claims by status
            result.ClaimsByStatus.Should().ContainKey("Submitted");
            result.ClaimsByStatus.Should().ContainKey("Approved");
            result.ClaimsByStatus.Should().ContainKey("Paid");
            result.ClaimsByStatus["Submitted"].Should().Be(1);
            result.ClaimsByStatus["Approved"].Should().Be(1);
            result.ClaimsByStatus["Paid"].Should().Be(1);

            // Test claims by type
            result.ClaimsByType.Should().ContainKey("Medical");
            result.ClaimsByType.Should().ContainKey("Travel");
            result.ClaimsByType["Medical"].Should().Be(2);
            result.ClaimsByType["Travel"].Should().Be(1);

            // Test amount by category
            result.AmountByCategory.Should().ContainKey("Medical");
            result.AmountByCategory.Should().ContainKey("Travel");
            result.AmountByCategory["Medical"].Should().Be(250.00m);
            result.AmountByCategory["Travel"].Should().Be(200.00m);

            // Test claims by month
            result.ClaimsByMonth.Should().ContainKey("2024-01");
            result.ClaimsByMonth.Should().ContainKey("2024-02");
            result.ClaimsByMonth["2024-01"].Should().Be(2);
            result.ClaimsByMonth["2024-02"].Should().Be(1);

            // Test top claimants - The service creates separate entries for each claim, not grouped by user
            result.TopClaimants.Should().HaveCount(3); // Each claim creates a separate entry
            result.TopClaimants.Should().Contain(t => t.UserName == "Jane Smith" && t.TotalAmount == 200.00m);
            result.TopClaimants.Should().Contain(t => t.UserName == "John Doe" && t.TotalAmount == 150.00m);
            result.TopClaimants.Should().Contain(t => t.UserName == "John Doe" && t.TotalAmount == 100.00m);

            // Test average processing time
            result.AverageProcessingTime.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetUserStatsAsync_ShouldReturnUserSpecificStats()
        {
            // Arrange
            var userId = 1;
            var userClaims = new List<Claim>
            {
                new Claim { ClaimId = 1, UserId = userId, Status = ClaimStatus.Draft, Amount = 50.00m },
                new Claim { ClaimId = 2, UserId = userId, Status = ClaimStatus.Submitted, Amount = 100.00m },
                new Claim { ClaimId = 3, UserId = userId, Status = ClaimStatus.Approved, Amount = 200.00m },
                new Claim { ClaimId = 4, UserId = userId, Status = ClaimStatus.Rejected, Amount = 75.00m },
                new Claim { ClaimId = 5, UserId = userId, Status = ClaimStatus.Paid, Amount = 150.00m }
            };

            _mockClaimRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(userClaims);

            // Act
            var result = await _dashboardService.GetUserStatsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalClaims.Should().Be(5);
            result.DraftClaims.Should().Be(1);
            result.SubmittedClaims.Should().Be(1);
            result.ApprovedClaims.Should().Be(1);
            result.RejectedClaims.Should().Be(1);
            result.PaidClaims.Should().Be(1);
            result.PendingClaims.Should().Be(1); // Only submitted claims
            result.TotalAmount.Should().Be(575.00m);
            result.ApprovedAmount.Should().Be(200.00m);
        }

        [Fact]
        public async Task GetRecentClaimsAsync_EmployeeRole_ShouldReturnUserClaims()
        {
            // Arrange
            var userId = 1;
            var userRole = "Employee";
            var userClaims = new List<Claim>
            {
                new Claim 
                { 
                    ClaimId = 1, 
                    UserId = userId, 
                    ClaimNumber = "CLM202412240001",
                    Title = "Medical Expense",
                    Status = ClaimStatus.Submitted, 
                    Amount = 100.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Category = new ExpenseCategory { Name = "Medical" },
                    User = new User { FirstName = "John", LastName = "Doe" }
                },
                new Claim 
                { 
                    ClaimId = 2, 
                    UserId = userId, 
                    ClaimNumber = "CLM202412240002",
                    Title = "Travel Expense",
                    Status = ClaimStatus.Approved, 
                    Amount = 200.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Category = new ExpenseCategory { Name = "Travel" },
                    User = new User { FirstName = "John", LastName = "Doe" }
                }
            };

            _mockClaimRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(userClaims);

            // Act
            var result = await _dashboardService.GetRecentClaimsAsync(userId, userRole);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var recentClaimsList = result.ToList();
            recentClaimsList[0].ClaimId.Should().Be(1); // Most recent first
            recentClaimsList[0].Title.Should().Be("Medical Expense");
            recentClaimsList[1].ClaimId.Should().Be(2);
            recentClaimsList[1].Title.Should().Be("Travel Expense");
        }

        [Fact]
        public async Task GetRecentClaimsAsync_ManagerRole_ShouldReturnAllClaims()
        {
            // Arrange
            var userId = 2;
            var userRole = "Manager";
            var allClaims = new List<Claim>
            {
                new Claim 
                { 
                    ClaimId = 1, 
                    UserId = 1, 
                    ClaimNumber = "CLM202412240001",
                    Title = "Medical Expense",
                    Status = ClaimStatus.Submitted, 
                    Amount = 100.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Category = new ExpenseCategory { Name = "Medical" },
                    User = new User { FirstName = "John", LastName = "Doe" }
                },
                new Claim 
                { 
                    ClaimId = 2, 
                    UserId = 3, 
                    ClaimNumber = "CLM202412240002",
                    Title = "Office Supplies",
                    Status = ClaimStatus.UnderReview, 
                    Amount = 50.00m,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    Category = new ExpenseCategory { Name = "Office" },
                    User = new User { FirstName = "Jane", LastName = "Smith" }
                }
            };

            _mockClaimRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allClaims);

            // Act
            var result = await _dashboardService.GetRecentClaimsAsync(userId, userRole);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var recentClaimsList = result.ToList();
            recentClaimsList[0].ClaimId.Should().Be(1); // Most recent first
            recentClaimsList[0].UserName.Should().Be("John Doe");
            recentClaimsList[1].ClaimId.Should().Be(2);
            recentClaimsList[1].UserName.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithOverdueClaims_ShouldCalculateCorrectly()
        {
            // Arrange
            var userId = 1;
            var userRole = "Manager";
            var allClaims = new List<Claim>
            {
                new Claim 
                { 
                    ClaimId = 1, 
                    Status = ClaimStatus.Submitted, 
                    Amount = 100.00m,
                    SLADueDate = DateTime.UtcNow.AddDays(-1) // Overdue
                },
                new Claim 
                { 
                    ClaimId = 2, 
                    Status = ClaimStatus.UnderReview, 
                    Amount = 200.00m,
                    SLADueDate = DateTime.UtcNow.AddDays(-2) // Overdue
                },
                new Claim 
                { 
                    ClaimId = 3, 
                    Status = ClaimStatus.Paid, 
                    Amount = 150.00m,
                    SLADueDate = DateTime.UtcNow.AddDays(-3) // Not overdue (already paid)
                }
            };

            _mockClaimRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allClaims);
            _mockClaimRepository.Setup(x => x.GetPendingApprovalsAsync(userId))
                .ReturnsAsync(new List<Claim>());

            // Act
            var result = await _dashboardService.GetDashboardStatsAsync(userId, userRole);

            // Assert
            result.Should().NotBeNull();
            result.OverdueClaims.Should().Be(2); // Only submitted and under review claims that are overdue
        }
    }
}