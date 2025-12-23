using ClaimsManagement.Business.DTOs.ClaimComment;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IClaimCommentService
    {
        Task<CommentResponseDto> AddCommentAsync(int claimId, string comment, int userId, bool isInternal = false);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByClaimIdAsync(int claimId);
        Task<CommentResponseDto?> GetCommentByIdAsync(int commentId);
        Task DeleteCommentAsync(int commentId);
    }
}