using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(20)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public ClaimType ClaimType { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? IncidentDate { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime? SLADueDate { get; set; }

        public bool IsEscalated { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ExpenseCategory Category { get; set; } = null!;
        public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
        public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
        public ICollection<ClaimApproval> Approvals { get; set; } = new List<ClaimApproval>();
        public ICollection<ClaimComment> Comments { get; set; } = new List<ClaimComment>();
        public ClaimPayment? Payment { get; set; }
    }
}