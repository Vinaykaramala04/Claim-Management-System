using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.ClaimApproval
{
    public class ApprovalResponseDto
    {
        public int ApprovalId { get; set; }
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; }
        public int ApprovalLevel { get; set; }
        public string? Comments { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}