using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Models
{
    public class ClaimApproval
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public int ApproverId { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required]
        public int ApprovalLevel { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Claim Claim { get; set; } = null!;
        public User Approver { get; set; } = null!;
    }
}