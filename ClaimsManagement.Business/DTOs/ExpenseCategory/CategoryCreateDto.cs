using System.ComponentModel.DataAnnotations;

namespace ClaimsManagement.Business.DTOs.ExpenseCategory
{
    public class CategoryCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxAmount { get; set; }

        public bool RequiresApproval { get; set; } = true;
    }
}