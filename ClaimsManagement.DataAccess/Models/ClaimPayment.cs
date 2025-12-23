using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Models
{
    public class ClaimPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public int ProcessedBy { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Claim Claim { get; set; } = null!;
        public User ProcessedByUser { get; set; } = null!;
    }
}