using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetActiveAsync()
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Name == name && d.IsActive);
        }
    }
}