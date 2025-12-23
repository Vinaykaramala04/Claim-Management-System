using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.Claim
{
    public class ClaimListDto
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ClaimStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}