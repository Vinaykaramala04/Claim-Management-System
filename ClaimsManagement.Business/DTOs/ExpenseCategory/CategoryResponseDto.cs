namespace ClaimsManagement.Business.DTOs.ExpenseCategory
{
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}