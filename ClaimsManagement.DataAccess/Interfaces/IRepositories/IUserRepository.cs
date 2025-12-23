using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailWithDepartmentAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(ClaimsManagement.DataAccess.Enum.UserRole role);
        Task<IEnumerable<User>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<User>> GetAllWithDepartmentAsync();
    }
}