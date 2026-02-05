using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ClaimsManagement.Business.Services;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Tests.Services
{
    public class ClaimApprovalServiceTests
    {
        private readonly Mock<IClaimApprovalRepository> _mockApprovalRepository;
        private readonly Mock<IClaimRepository> _mockClaimRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<ClaimApprovalService>> _mockLogger;
        private readonly ClaimApprovalService _approvalService;

        public ClaimApprovalServiceTests()
        {
            _mockApprovalRepository = new Mock<IClaimApprovalRepository>();
            _mockClaimRepository = new Mock<IClaimRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<ClaimApprovalService>>();

            _approvalService = new ClaimApprovalService(
                _mockApprovalRepository.Object,
                _mockClaimRepository.Object,
                _mockNotificationService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ProcessApprovalAsync_ApproveValidClaim_ShouldApproveSuccessfully()
        {
            // Arrange
            var claimId = 1;
            var approverId = 2;
            var status = ApprovalStatus.Approved;
            var comments = "Approved after review";

            var claim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                UserId = 3,
                Amount = 150.00m,
                Status = ClaimStatus.UnderReview
            };

            var approval = new ClaimApproval
            {
                ApprovalId = 1,
                ClaimId = claimId,
                ApproverId = approverId,
                Status = status,
                ApprovalLevel = 1,
                Comments = comments,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(claim);
            _mockApprovalRepository.Setup(x => x.AddAsync(It.IsAny<ClaimApproval>()))
                .ReturnsAsync(approval);
            _mockClaimRepository.Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .ReturnsAsync(claim);

            // Act
            var result = await _approvalService.ProcessApprovalAsync(claimId, status, approverId, comments);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ApprovalStatus.Approved);
            result.Comments.Should().Be(comments);
            result.ApprovedAt.Should().NotBeNull();

            // Verify claim status was updated
            _mockClaimRepository.Verify(x => x.UpdateAsync(It.Is<Claim>(c => 
                c.Status == ClaimStatus.Approved && 
                c.ApprovedAt.HasValue)), Times.Once);

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(
                claim.UserId,
                "Claim Approved",
                It.IsAny<string>(),
                NotificationType.ClaimApproved,
                claimId), Times.Once);
        }

        [Fact]
        public async Task ProcessApprovalAsync_RejectValidClaim_ShouldRejectSuccessfully()
        {
            // Arrange
            var claimId = 1;
            var approverId = 2;
            var status = ApprovalStatus.Rejected;
            var comments = "Insufficient documentation";

            var claim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                UserId = 3,
                Amount = 150.00m,
                Status = ClaimStatus.UnderReview
            };

            var approval = new ClaimApproval
            {
                ApprovalId = 1,
                ClaimId = claimId,
                ApproverId = approverId,
                Status = status,
                ApprovalLevel = 1,
                Comments = comments,
                ApprovedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(claim);
            _mockApprovalRepository.Setup(x => x.AddAsync(It.IsAny<ClaimApproval>()))
                .ReturnsAsync(approval);
            _mockClaimRepository.Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .ReturnsAsync(claim);

            // Act
            var result = await _approvalService.ProcessApprovalAsync(claimId, status, approverId, comments);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ApprovalStatus.Rejected);
            result.Comments.Should().Be(comments);
            result.ApprovedAt.Should().BeNull();

            // Verify claim status was updated
            _mockClaimRepository.Verify(x => x.UpdateAsync(It.Is<Claim>(c => 
                c.Status == ClaimStatus.Rejected)), Times.Once);

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(
                claim.UserId,
                "Claim Rejected",
                It.IsAny<string>(),
                NotificationType.ClaimRejected,
                claimId), Times.Once);
        }

        [Fact]
        public async Task ProcessApprovalAsync_NonExistentClaim_ShouldThrowArgumentException()
        {
            // Arrange
            var claimId = 999;
            var approverId = 2;
            var status = ApprovalStatus.Approved;

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync((Claim?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _approvalService.ProcessApprovalAsync(claimId, status, approverId));
        }

        [Fact]
        public async Task GetPendingApprovalsAsync_ValidApproverId_ShouldReturnPendingApprovals()
        {
            // Arrange
            var approverId = 2;
            var approvals = new List<ClaimApproval>
            {
                new ClaimApproval 
                { 
                    ApprovalId = 1, 
                    ClaimId = 1, 
                    ApproverId = approverId, 
                    Status = ApprovalStatus.Pending,
                    Claim = new Claim { ClaimNumber = "CLM202412240001" },
                    Approver = new User { FirstName = "John", LastName = "Manager" }
                },
                new ClaimApproval 
                { 
                    ApprovalId = 2, 
                    ClaimId = 2, 
                    ApproverId = approverId, 
                    Status = ApprovalStatus.Approved, // This should be filtered out
                    Claim = new Claim { ClaimNumber = "CLM202412240002" },
                    Approver = new User { FirstName = "John", LastName = "Manager" }
                },
                new ClaimApproval 
                { 
                    ApprovalId = 3, 
                    ClaimId = 3, 
                    ApproverId = approverId, 
                    Status = ApprovalStatus.Pending,
                    Claim = new Claim { ClaimNumber = "CLM202412240003" },
                    Approver = new User { FirstName = "John", LastName = "Manager" }
                }
            };

            _mockApprovalRepository.Setup(x => x.GetByApproverIdAsync(approverId))
                .ReturnsAsync(approvals);

            // Act
            var result = await _approvalService.GetPendingApprovalsAsync(approverId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only pending approvals
            result.All(a => a.Status == ApprovalStatus.Pending).Should().BeTrue();
        }

        [Fact]
        public async Task GetApprovalsByClaimIdAsync_ValidClaimId_ShouldReturnAllApprovals()
        {
            // Arrange
            var claimId = 1;
            var approvals = new List<ClaimApproval>
            {
                new ClaimApproval 
                { 
                    ApprovalId = 1, 
                    ClaimId = claimId, 
                    ApproverId = 2, 
                    Status = ApprovalStatus.Approved,
                    Claim = new Claim { ClaimNumber = "CLM202412240001" },
                    Approver = new User { FirstName = "John", LastName = "Manager" }
                },
                new ClaimApproval 
                { 
                    ApprovalId = 2, 
                    ClaimId = claimId, 
                    ApproverId = 3, 
                    Status = ApprovalStatus.Pending,
                    Claim = new Claim { ClaimNumber = "CLM202412240001" },
                    Approver = new User { FirstName = "Jane", LastName = "Director" }
                }
            };

            _mockApprovalRepository.Setup(x => x.GetByClaimIdAsync(claimId))
                .ReturnsAsync(approvals);

            // Act
            var result = await _approvalService.GetApprovalsByClaimIdAsync(claimId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(a => a.ClaimId == claimId).Should().BeTrue();
        }

        [Fact]
        public async Task GetApprovalByIdAsync_ValidId_ShouldReturnApproval()
        {
            // Arrange
            var approvalId = 1;
            var approval = new ClaimApproval
            {
                ApprovalId = approvalId,
                ClaimId = 1,
                ApproverId = 2,
                Status = ApprovalStatus.Approved,
                Comments = "Approved",
                Claim = new Claim { ClaimNumber = "CLM202412240001" },
                Approver = new User { FirstName = "John", LastName = "Manager" }
            };

            _mockApprovalRepository.Setup(x => x.GetByIdAsync(approvalId))
                .ReturnsAsync(approval);

            // Act
            var result = await _approvalService.GetApprovalByIdAsync(approvalId);

            // Assert
            result.Should().NotBeNull();
            result!.ApprovalId.Should().Be(approvalId);
            result.Status.Should().Be(ApprovalStatus.Approved);
            result.Comments.Should().Be("Approved");
            result.ApproverName.Should().Be("John Manager");
        }

        [Fact]
        public async Task GetApprovalByIdAsync_NonExistentId_ShouldReturnNull()
        {
            // Arrange
            var approvalId = 999;
            _mockApprovalRepository.Setup(x => x.GetByIdAsync(approvalId))
                .ReturnsAsync((ClaimApproval?)null);

            // Act
            var result = await _approvalService.GetApprovalByIdAsync(approvalId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ProcessApprovalAsync_WithoutComments_ShouldProcessSuccessfully()
        {
            // Arrange
            var claimId = 1;
            var approverId = 2;
            var status = ApprovalStatus.Approved;

            var claim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                UserId = 3,
                Amount = 150.00m,
                Status = ClaimStatus.UnderReview
            };

            var approval = new ClaimApproval
            {
                ApprovalId = 1,
                ClaimId = claimId,
                ApproverId = approverId,
                Status = status,
                ApprovalLevel = 1,
                Comments = null,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(claim);
            _mockApprovalRepository.Setup(x => x.AddAsync(It.IsAny<ClaimApproval>()))
                .ReturnsAsync(approval);
            _mockClaimRepository.Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .ReturnsAsync(claim);

            // Act
            var result = await _approvalService.ProcessApprovalAsync(claimId, status, approverId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(ApprovalStatus.Approved);
            result.Comments.Should().BeNull();

            // Verify approval was created without comments
            _mockApprovalRepository.Verify(x => x.AddAsync(It.Is<ClaimApproval>(a => 
                a.Comments == null)), Times.Once);
        }

        [Theory]
        [InlineData(ApprovalStatus.Approved, ClaimStatus.Approved)]
        [InlineData(ApprovalStatus.Rejected, ClaimStatus.Rejected)]
        public async Task ProcessApprovalAsync_DifferentStatuses_ShouldUpdateClaimStatusCorrectly(
            ApprovalStatus approvalStatus, ClaimStatus expectedClaimStatus)
        {
            // Arrange
            var claimId = 1;
            var approverId = 2;

            var claim = new Claim
            {
                ClaimId = claimId,
                ClaimNumber = "CLM202412240001",
                UserId = 3,
                Amount = 150.00m,
                Status = ClaimStatus.UnderReview
            };

            var approval = new ClaimApproval
            {
                ApprovalId = 1,
                ClaimId = claimId,
                ApproverId = approverId,
                Status = approvalStatus,
                ApprovalLevel = 1,
                CreatedAt = DateTime.UtcNow
            };

            _mockClaimRepository.Setup(x => x.GetByIdAsync(claimId))
                .ReturnsAsync(claim);
            _mockApprovalRepository.Setup(x => x.AddAsync(It.IsAny<ClaimApproval>()))
                .ReturnsAsync(approval);
            _mockClaimRepository.Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .ReturnsAsync(claim);

            // Act
            var result = await _approvalService.ProcessApprovalAsync(claimId, approvalStatus, approverId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(approvalStatus);

            // Verify claim status was updated correctly
            _mockClaimRepository.Verify(x => x.UpdateAsync(It.Is<Claim>(c => 
                c.Status == expectedClaimStatus)), Times.Once);
        }
    }
}