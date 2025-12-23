using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.Claim
{
    public class ClaimResponseDto
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public ClaimType ClaimType { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ClaimStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? SLADueDate { get; set; }
        public bool IsEscalated { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int DocumentCount { get; set; }
        public string? StatusComments { get; set; }
        public DateTime? LastStatusChangeDate { get; set; }
        public string? LastStatusChangedBy { get; set; }
    }
}