using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.Claim
{
    public class ClaimUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public Priority Priority { get; set; }

        public DateTime? IncidentDate { get; set; }
    }
}