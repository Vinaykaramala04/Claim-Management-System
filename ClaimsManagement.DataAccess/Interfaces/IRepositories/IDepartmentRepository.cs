using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.DataAccess.Interfaces.IRepositories
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<IEnumerable<Department>> GetActiveAsync();
        Task<Department?> GetByNameAsync(string name);
    }
}