using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.User
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public int? DepartmentId { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;
    }
}