using Microsoft.EntityFrameworkCore;
using ClaimsManagement.DataAccess.Data;
using ClaimsManagement.DataAccess.Models;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;

namespace ClaimsManagement.DataAccess.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ClaimsManagementDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailWithDepartmentAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(ClaimsManagement.DataAccess.Enum.UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == role && u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByDepartmentAsync(int departmentId)
        {
            return await _dbSet
                .Where(u => u.DepartmentId == departmentId && u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllWithDepartmentAsync()
        {
            return await _dbSet
                .Include(u => u.Department)
                .ToListAsync();
        }
    }
}