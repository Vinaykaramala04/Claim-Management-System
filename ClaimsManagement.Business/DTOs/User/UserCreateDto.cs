using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.User
{
    public class UserCreateDto
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

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public int? DepartmentId { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}