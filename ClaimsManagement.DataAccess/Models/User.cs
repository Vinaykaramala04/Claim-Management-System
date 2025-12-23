using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.DataAccess.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public int? DepartmentId { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Department? Department { get; set; }
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<ClaimApproval> ClaimApprovals { get; set; } = new List<ClaimApproval>();
        public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
    }
}