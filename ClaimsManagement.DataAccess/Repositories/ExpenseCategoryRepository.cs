using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class ExpenseCategoryRepository : BaseRepository<ExpenseCategory>, IExpenseCategoryRepository
    {
        public ExpenseCategoryRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExpenseCategory>> GetActiveAsync()
        {
            return await _dbSet
                .Where(ec => ec.IsActive)
                .OrderBy(ec => ec.Name)
                .ToListAsync();
        }

        public async Task<ExpenseCategory?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ec => ec.Name == name && ec.IsActive);
        }
    }
}