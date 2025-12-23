using ClaimsManagement.Business.DTOs.Auth;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<string> GenerateJwtTokenAsync(int userId, string email, string role);
        Task<bool> ValidateTokenAsync(string token);
    }
}