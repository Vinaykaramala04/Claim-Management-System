namespace ClaimsManagement.Business.DTOs.ClaimComment
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int ClaimId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}