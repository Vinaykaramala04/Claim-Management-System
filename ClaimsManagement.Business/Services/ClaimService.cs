using ClaimsManagement.Business.DTOs.Claim;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IExpenseCategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ClaimService> _logger;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public ClaimService(
            IClaimRepository claimRepository, 
            IExpenseCategoryRepository categoryRepository, 
            IUserRepository userRepository,
            ILogger<ClaimService> logger,
            IEmailNotificationService emailNotificationService,
            IEmailService emailService,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _claimRepository = claimRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _logger = logger;
            _emailNotificationService = emailNotificationService;
            _emailService = emailService;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<ClaimResponseDto> CreateClaimAsync(ClaimCreateDto request, int userId)
        {
            try
            {
                _logger.LogInformation("Creating claim for user {UserId}", userId);
                await ValidateClaimRequestAsync(request);
                
                var claimNumber = await GenerateClaimNumberAsync();
                var slaDate = CalculateSLADate(request.Priority);

            var claim = new Claim
            {
                ClaimNumber = claimNumber,
                UserId = userId,
                ClaimType = request.ClaimType,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                Amount = request.Amount,
                Status = ClaimStatus.Submitted,
                Priority = request.Priority,
                IncidentDate = request.IncidentDate,
                SubmittedAt = DateTime.UtcNow,
                SLADueDate = slaDate,
                CreatedAt = DateTime.UtcNow
            };

                var createdClaim = await _claimRepository.AddAsync(claim);
                // Reload the claim with related entities
                var claimWithRelations = await _claimRepository.GetByIdAsync(createdClaim.ClaimId);
                
                // Create notification for claim submission
                await _notificationService.CreateNotificationAsync(
                    userId, 
                    "Claim Submitted Successfully", 
                    $"Your claim {claimNumber} has been submitted and is awaiting review.", 
                    NotificationType.ClaimSubmitted,
                    claimWithRelations.ClaimId
                );
                
                // Send notifications and log audit
                await _emailNotificationService.SendClaimCreatedAsync(claimWithRelations!);
                
                // Send claim submitted confirmation email
                try
                {
                    // Get user email from user repository
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendClaimSubmittedEmailAsync(user.Email, claimNumber);
                        _logger.LogInformation("Claim submitted email sent to {Email} for claim {ClaimNumber}", user.Email, claimNumber);
                    }
                    else
                    {
                        _logger.LogWarning("User email not found for user {UserId}, cannot send claim submitted email", userId);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send claim submitted email for claim {ClaimNumber}", claimNumber);
                }
                
                await _auditService.LogClaimCreatedAsync(claimWithRelations!.ClaimId, userId, 
                    $"Claim created: {claimWithRelations.Title}, Amount: ${claimWithRelations.Amount}");
                
                _logger.LogInformation("Claim {ClaimNumber} created successfully for user {UserId}", claimNumber, userId);
                return await MapToResponseDto(claimWithRelations!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create claim for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ClaimResponseDto?> GetClaimByIdAsync(int claimId)
        {
            try
            {
                if (claimId <= 0)
                    throw new ArgumentException("Invalid claim ID");
                    
                var claim = await _claimRepository.GetByIdAsync(claimId);
                return claim != null ? await MapToResponseDto(claim) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get claim {ClaimId}", claimId);
                throw;
            }
        }

        public async Task<ClaimResponseDto?> GetClaimByNumberAsync(string claimNumber)
        {
            var claim = await _claimRepository.GetByClaimNumberAsync(claimNumber);
            return claim != null ? await MapToResponseDto(claim) : null;
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetClaimsByUserAsync(int userId)
        {
            var claims = await _claimRepository.GetByUserIdAsync(userId);
            var responseDtos = new List<ClaimResponseDto>();

            foreach (var claim in claims)
            {
                responseDtos.Add(await MapToResponseDto(claim));
            }

            return responseDtos;
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetClaimsByStatusAsync(ClaimStatus status)
        {
            var claims = await _claimRepository.GetByStatusAsync(status);
            var responseDtos = new List<ClaimResponseDto>();

            foreach (var claim in claims)
            {
                responseDtos.Add(await MapToResponseDto(claim));
            }

            return responseDtos;
        }

        public async Task<ClaimResponseDto> UpdateClaimStatusAsync(int claimId, ClaimStatus status, int updatedBy, string? comments = null)
        {
            try
            {
                _logger.LogInformation("Updating claim {ClaimId} status to {Status} by user {UpdatedBy}", claimId, status, updatedBy);
                
                var claim = await _claimRepository.GetByIdAsync(claimId);
                if (claim == null)
                    throw new ArgumentException("Claim not found");

                ValidateStatusTransition(claim.Status, status);

                var oldStatus = claim.Status;
                claim.Status = status;
                claim.UpdatedAt = DateTime.UtcNow;

                if (status == ClaimStatus.Approved)
                    claim.ApprovedAt = DateTime.UtcNow;
                else if (status == ClaimStatus.Paid)
                    claim.PaidAt = DateTime.UtcNow;

                // Create status history record with comments
                var statusHistory = new ClaimStatusHistory
                {
                    ClaimId = claimId,
                    FromStatus = oldStatus,
                    ToStatus = status,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = updatedBy,
                    Comments = comments
                };
                claim.StatusHistory.Add(statusHistory);

                var updatedClaim = await _claimRepository.UpdateAsync(claim);
                
                // Create notifications based on status change
                await CreateStatusChangeNotificationsAsync(updatedClaim, oldStatus, status, updatedBy);
                
                // Send notifications and log audit
                await _emailNotificationService.SendClaimStatusUpdateAsync(updatedClaim, oldStatus, status);
                await _auditService.LogStatusChangeAsync(claimId, updatedBy, oldStatus.ToString(), status.ToString(), comments);
                
                _logger.LogInformation("Claim {ClaimId} status updated from {OldStatus} to {NewStatus}", claimId, oldStatus, status);
                return await MapToResponseDto(updatedClaim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update claim {ClaimId} status", claimId);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetPendingApprovalsAsync(int approverId)
        {
            var claims = await _claimRepository.GetPendingApprovalsAsync(approverId);
            var responseDtos = new List<ClaimResponseDto>();

            foreach (var claim in claims)
            {
                responseDtos.Add(await MapToResponseDto(claim));
            }

            return responseDtos;
        }

        public async Task<string> GenerateClaimNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var count = await _claimRepository.CountAsync(c => c.CreatedAt.Year == year && c.CreatedAt.Month == month);
            return $"CLM{year}{month:D2}{(count + 1):D4}";
        }

        private DateTime CalculateSLADate(Priority priority)
        {
            var businessDays = priority switch
            {
                Priority.Critical => 1,
                Priority.High => 3,
                Priority.Medium => 5,
                Priority.Low => 10,
                _ => 5
            };

            var slaDate = DateTime.UtcNow;
            var addedDays = 0;

            while (addedDays < businessDays)
            {
                slaDate = slaDate.AddDays(1);
                if (slaDate.DayOfWeek != DayOfWeek.Saturday && slaDate.DayOfWeek != DayOfWeek.Sunday)
                    addedDays++;
            }

            return slaDate;
        }

        private async Task ValidateClaimRequestAsync(ClaimCreateDto request)
        {
            if (request.Amount <= 0 || request.Amount > 100000)
                throw new ArgumentException("Claim amount must be between $1 and $100,000");
                
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
                throw new ArgumentException("Title is required and must be less than 200 characters");
                
            if (request.IncidentDate > DateTime.UtcNow)
                throw new ArgumentException("Incident date cannot be in the future");
                
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid expense category");
        }

        private void ValidateStatusTransition(ClaimStatus currentStatus, ClaimStatus newStatus)
        {
            var validTransitions = new Dictionary<ClaimStatus, ClaimStatus[]>
            {
                [ClaimStatus.Submitted] = new[] { ClaimStatus.UnderReview, ClaimStatus.Rejected },
                [ClaimStatus.UnderReview] = new[] { ClaimStatus.Approved, ClaimStatus.Rejected, ClaimStatus.MoreInfoRequired },
                [ClaimStatus.MoreInfoRequired] = new[] { ClaimStatus.UnderReview, ClaimStatus.Rejected },
                [ClaimStatus.Approved] = new[] { ClaimStatus.Paid },
                [ClaimStatus.Rejected] = new ClaimStatus[0],
                [ClaimStatus.Paid] = new ClaimStatus[0]
            };

            if (!validTransitions.ContainsKey(currentStatus) || !validTransitions[currentStatus].Contains(newStatus))
                throw new ArgumentException($"Invalid status transition from {currentStatus} to {newStatus}");
        }

        private async Task<ClaimResponseDto> MapToResponseDto(Claim claim)
        {
            // Get the latest status history entry for comments
            var latestStatusHistory = claim.StatusHistory?.OrderByDescending(h => h.ChangedAt).FirstOrDefault();
            
            // Also check ClaimApproval for rejection comments
            var rejectionApproval = claim.Approvals?.FirstOrDefault(a => a.Status == ApprovalStatus.Rejected);
            
            // Use status history comments first, then approval comments as fallback
            var statusComments = latestStatusHistory?.Comments ?? rejectionApproval?.Comments;
            var lastChangedBy = latestStatusHistory?.ChangedByUser != null ? 
                $"{latestStatusHistory.ChangedByUser.FirstName} {latestStatusHistory.ChangedByUser.LastName}" : 
                (rejectionApproval?.Approver != null ? $"{rejectionApproval.Approver.FirstName} {rejectionApproval.Approver.LastName}" : null);
            var lastChangeDate = latestStatusHistory?.ChangedAt ?? rejectionApproval?.ApprovedAt;
            
            return new ClaimResponseDto
            {
                ClaimId = claim.ClaimId,
                ClaimNumber = claim.ClaimNumber,
                ClaimType = claim.ClaimType,
                CategoryName = claim.Category?.Name ?? "Unknown",
                Title = claim.Title,
                Description = claim.Description,
                Amount = claim.Amount,
                Status = claim.Status,
                Priority = claim.Priority,
                IncidentDate = claim.IncidentDate,
                SubmittedAt = claim.SubmittedAt,
                ApprovedAt = claim.ApprovedAt,
                PaidAt = claim.PaidAt,
                SLADueDate = claim.SLADueDate,
                IsEscalated = claim.IsEscalated,
                UserName = claim.User != null ? $"{claim.User.FirstName} {claim.User.LastName}" : "Unknown",
                UserId = claim.UserId,
                DocumentCount = claim.Documents?.Count ?? 0,
                StatusComments = statusComments,
                LastStatusChangeDate = lastChangeDate,
                LastStatusChangedBy = lastChangedBy
            };
        }

        private async Task CreateStatusChangeNotificationsAsync(Claim claim, ClaimStatus oldStatus, ClaimStatus newStatus, int updatedBy)
        {
            try
            {
                // Notify claim owner about status changes
                var (title, message, notificationType) = GetNotificationContent(claim, oldStatus, newStatus);
                
                if (!string.IsNullOrEmpty(title))
                {
                    await _notificationService.CreateNotificationAsync(
                        claim.UserId,
                        title,
                        message,
                        notificationType,
                        claim.ClaimId
                    );
                }

                // Notify managers/agents for approval requirements
                if (newStatus == ClaimStatus.UnderReview)
                {
                    // Notify agents that a claim needs review
                    await _notificationService.CreateNotificationAsync(
                        updatedBy, // The agent who moved it to review
                        "Claim Requires Review",
                        $"Claim {claim.ClaimNumber} is now under review and requires your attention.",
                        NotificationType.ApprovalRequired
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create status change notifications for claim {ClaimId}", claim.ClaimId);
            }
        }

        private (string title, string message, NotificationType type) GetNotificationContent(Claim claim, ClaimStatus oldStatus, ClaimStatus newStatus)
        {
            return newStatus switch
            {
                ClaimStatus.UnderReview => (
                    "Claim Under Review",
                    $"Your claim {claim.ClaimNumber} is now under review by our team.",
                    NotificationType.ClaimUpdate
                ),
                ClaimStatus.Approved => (
                    "Claim Approved",
                    $"Great news! Your claim {claim.ClaimNumber} for ${claim.Amount:F2} has been approved.",
                    NotificationType.ClaimApproved
                ),
                ClaimStatus.Rejected => (
                    "Claim Rejected",
                    $"Your claim {claim.ClaimNumber} has been rejected. Please check the comments for details.",
                    NotificationType.ClaimRejected
                ),
                ClaimStatus.Paid => (
                    "Payment Processed",
                    $"Payment of ${claim.Amount:F2} for claim {claim.ClaimNumber} has been processed.",
                    NotificationType.ClaimPaid
                ),
                ClaimStatus.MoreInfoRequired => (
                    "Additional Information Required",
                    $"Your claim {claim.ClaimNumber} requires additional information. Please provide the requested documents.",
                    NotificationType.DocumentRequired
                ),
                _ => (string.Empty, string.Empty, NotificationType.ClaimUpdate)
            };
        }
    }
}