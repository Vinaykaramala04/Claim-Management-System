using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Models
{
    public class ClaimStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public ClaimStatus FromStatus { get; set; }

        [Required]
        public ClaimStatus ToStatus { get; set; }

        [Required]
        public int ChangedBy { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Claim Claim { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}