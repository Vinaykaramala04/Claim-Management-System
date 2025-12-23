using System.ComponentModel.DataAnnotations;

namespace ClaimsManagement.DataAccess.Models
{
    public class ClaimComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Claim Claim { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}