using ClaimsManagement.Business.DTOs.User;
using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(UserCreateDto request);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> UpdateUserAsync(int userId, UserUpdateDto request);
        Task DeactivateUserAsync(int userId);
        Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
        Task<string> HashPasswordAsync(string password);
    }
}