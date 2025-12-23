using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimsManagement.DataAccess.Models
{
    public class UserClaimStats
    {
        [Key]
        public int StatsId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int TotalClaims { get; set; } = 0;

        public int ApprovedClaims { get; set; } = 0;

        public int RejectedClaims { get; set; } = 0;

        public int PendingClaims { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal ApprovedAmount { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
    }
}