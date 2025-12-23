using ClaimsManagement.Business.DTOs.ExpenseCategory;
using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IExpenseCategoryService
    {
        Task<CategoryResponseDto> CreateCategoryAsync(string name, string? description, decimal? maxAmount, bool requiresApproval);
        Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, string name, string? description, decimal? maxAmount, bool requiresApproval);
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync();
        Task DeleteCategoryAsync(int categoryId);
    }
}