using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IExpenseCategoryRepository : IBaseRepository<ExpenseCategory>
    {
        Task<IEnumerable<ExpenseCategory>> GetActiveAsync();
        Task<ExpenseCategory?> GetByNameAsync(string name);
    }
}