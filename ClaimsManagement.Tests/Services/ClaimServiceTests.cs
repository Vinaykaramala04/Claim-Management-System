using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ClaimsManagement.Business.Services;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using ClaimsManagement.Business.DTOs.Claim;

namespace ClaimsManagement.Tests.Services
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimRepository> _mockClaimRepository;
        private readonly Mock<IExpenseCategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IEmailNotificationService> _mockEmailNotificationService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            _mockClaimRepository = new Mock<IClaimRepository>();
            _mockCategoryRepository = new Mock<IExpenseCategoryRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockEmailNotificationService = new Mock<IEmailNotificationService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockNotificationService = new Mock<INotificationService>();

            _claimService = new ClaimService(
                _mockClaimRepository.Object,
                _mockCategoryRepository.Object,
                _mockUserRepository.Object,
                _mockLogger.Object,
                _mockEmailNotificationService.Object,
                _mockEmailService.Object,
                _mockAuditService.Object,
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task CreateClaimAsync_ValidRequest_ShouldCreateClaimSuccessfully()
        {
            // Arrange
            var userId = 1;
            var request = new ClaimCreateDto
            {
                ClaimType = ClaimType.Medical,
                CategoryId = 1,
                Title = "Medical Expense",
                Description = "Doctor visit",
                Amount = 150.00m,
                Priority = Priority.Medium,
                IncidentDate = DateTime.UtcNow.AddDays(-1)
            };

            var category = new ExpenseCategory { CategoryId = 1, Name = "Medical" };
            var user = new User { UserId = userId, Email = "test@example.com", FirstName = "John", LastName = "Doe" };
            var createdClaim = new Claim
            {
                ClaimId = 1,
                ClaimNumber = "CLM202412240001",
                UserId = userId,
                ClaimType = request.ClaimType,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                Amount = request.Amount,
                Status = ClaimStatus.Submitted,
                Priority = request.Priority,
                IncidentDate = request.IncidentDate,
                Category = category,
                User = user
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(request.CategoryId))
                .ReturnsAsync(category);
            _mockClaimRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Claim, bool>>>()))
                .ReturnsAsync(0);
            _mockClaimRepository.Setup(x => x.AddAsync(It.IsAny<Claim>()))
                .ReturnsAsync(createdClaim);
            _mockClaimRepository.Setup(x => x.GetByIdAsync(createdClaim.ClaimId))
                .ReturnsAsync(createdClaim);
            _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _claimService.CreateClaimAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.ClaimNumber.Should().StartWith("CLM");
            result.Title.Should().Be(request.Title);
            result.Amount.Should().Be(request.Amount);
            result.Status.Should().Be(ClaimStatus.Submitted);
            
            _mockClaimRepository.Verify(x => x.AddAsync(It.IsAny<Claim>()), Times.Once);
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(
                userId, 
                "Claim Submitted Successfully", 
                It.IsAny<string>(), 
                NotificationType.ClaimSubmitted,
                It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task CreateClaimAsync_InvalidAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 1;
            var request = new ClaimCreateDto
            {
                ClaimType = ClaimType.Medical,
                CategoryId = 1,
                Title = "Medical Expense",
                Description = "Doctor visit",
                Amount = -50.00m, // Invalid amount
                Priority = Priority.Medium,
                IncidentDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _claimService.CreateClaimAsync(request, userId));
        }

        [Fact]
        public async Task GetClaimByIdAsync_ValidId_ShouldReturnClaim()
        {
            // Arrange
            var claimId = 1;
            var claim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                Title = "Test Claim",
                Amount = 100.00m,
                Status = ClaimStatus.Submitted,
                Category = new ExpenseCategory { Name = "Medical" },
                User = new User { FirstName = "John", LastName = "Doe" }
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(claim);

            // Act
            var result = await _claimService.GetClaimByIdAsync(claimId);

            // Assert
            result.Should().NotBeNull();
            result!.ClaimId.Should().Be(claimId);
            result.Title.Should().Be(claim.Title);
            result.Amount.Should().Be(claim.Amount);
        }

        [Fact]
        public async Task UpdateClaimStatusAsync_ValidTransition_ShouldUpdateSuccessfully()
        {
            // Arrange
            var claimId = 1;
            var newStatus = ClaimStatus.Approved;
            var updatedBy = 2;
            var comments = "Approved after review";

            var existingClaim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                Status = ClaimStatus.UnderReview,
                StatusHistory = new List<ClaimStatusHistory>(),
                Category = new ExpenseCategory { Name = "Medical" },
                User = new User { FirstName = "John", LastName = "Doe" }
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(existingClaim);
            _mockClaimRepository.Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .ReturnsAsync(existingClaim);

            // Act
            var result = await _claimService.UpdateClaimStatusAsync(claimId, newStatus, updatedBy, comments);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(newStatus);
            
            _mockClaimRepository.Verify(x => x.UpdateAsync(It.Is<Claim>(c => 
                c.Status == newStatus && 
                c.StatusHistory.Any(h => h.Comments == comments))), Times.Once);
        }

        [Fact]
        public async Task GenerateClaimNumberAsync_ShouldGenerateCorrectFormat()
        {
            // Arrange
            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;
            var expectedCount = 5;

            _mockClaimRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Claim, bool>>>()))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _claimService.GenerateClaimNumberAsync();

            // Assert
            result.Should().StartWith("CLM");
            result.Should().Contain(currentYear.ToString());
            result.Should().Contain(currentMonth.ToString("D2"));
            result.Should().EndWith((expectedCount + 1).ToString("D4"));
        }
    }
}