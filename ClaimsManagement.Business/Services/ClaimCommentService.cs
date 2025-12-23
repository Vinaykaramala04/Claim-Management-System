using ClaimsManagement.Business.DTOs.ClaimComment;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.Services
{
    public class ClaimCommentService : IClaimCommentService
    {
        private readonly IClaimCommentRepository _commentRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly INotificationService _notificationService;

        public ClaimCommentService(IClaimCommentRepository commentRepository, IClaimRepository claimRepository, INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _claimRepository = claimRepository;
            _notificationService = notificationService;
        }

        public async Task<CommentResponseDto> AddCommentAsync(int claimId, string comment, int userId, bool isInternal = false)
        {
            var claimComment = new ClaimComment
            {
                ClaimId = claimId,
                UserId = userId,
                Comment = comment,
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            var savedComment = await _commentRepository.AddAsync(claimComment);
            
            // Send notification to claim owner if comment is not internal and not from the owner
            if (!isInternal)
            {
                var claim = await _claimRepository.GetByIdAsync(claimId);
                if (claim != null && claim.UserId != userId)
                {
                    await _notificationService.CreateNotificationAsync(
                        claim.UserId,
                        "New Comment on Your Claim",
                        $"A new comment has been added to your claim {claim.ClaimNumber}: {comment}",
                        NotificationType.ClaimUpdate
                    );
                }
            }
            
            return MapToResponseDto(savedComment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByClaimIdAsync(int claimId)
        {
            var comments = await _commentRepository.GetByClaimIdAsync(claimId);
            return comments.Select(MapToResponseDto);
        }

        public async Task<CommentResponseDto?> GetCommentByIdAsync(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            return comment != null ? MapToResponseDto(comment) : null;
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment != null)
            {
                await _commentRepository.DeleteAsync(comment);
            }
        }

        private CommentResponseDto MapToResponseDto(ClaimComment comment)
        {
            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                ClaimId = comment.ClaimId,
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                UserName = comment.User != null ? $"{comment.User.FirstName} {comment.User.LastName}" : "Unknown",
                CreatedAt = comment.CreatedAt
            };
        }
    }
}