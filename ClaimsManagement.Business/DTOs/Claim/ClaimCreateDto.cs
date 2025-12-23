using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.Claim
{
    public class ClaimCreateDto
    {
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
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? IncidentDate { get; set; }
    }
}