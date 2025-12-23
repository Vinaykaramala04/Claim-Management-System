using ClaimsManagement.Business.DTOs.ExpenseCategory;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.Business.Services
{
    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _categoryRepository;

        public ExpenseCategoryService(IExpenseCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(string name, string? description, decimal? maxAmount, bool requiresApproval)
        {
            var category = new ExpenseCategory
            {
                Name = name,
                Description = description,
                MaxAmount = maxAmount,
                RequiresApproval = requiresApproval,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var savedCategory = await _categoryRepository.AddAsync(category);
            return MapToResponseDto(savedCategory);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, string name, string? description, decimal? maxAmount, bool requiresApproval)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            category.Name = name;
            category.Description = description;
            category.MaxAmount = maxAmount;
            category.RequiresApproval = requiresApproval;

            var updatedCategory = await _categoryRepository.UpdateAsync(category);
            return MapToResponseDto(updatedCategory);
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            return category != null ? MapToResponseDto(category) : null;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveAsync();
            return categories.Select(MapToResponseDto);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category != null)
            {
                category.IsActive = false;
                await _categoryRepository.UpdateAsync(category);
            }
        }

        private CategoryResponseDto MapToResponseDto(ExpenseCategory category)
        {
            return new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                MaxAmount = category.MaxAmount,
                RequiresApproval = category.RequiresApproval,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }
    }
}