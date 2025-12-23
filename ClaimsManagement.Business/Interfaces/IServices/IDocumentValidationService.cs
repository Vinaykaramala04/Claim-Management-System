using Microsoft.AspNetCore.Http;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IDocumentValidationService
    {
        Task<bool> ValidateDocumentAsync(IFormFile file);
        Task<bool> IsValidFileTypeAsync(string fileName);
        Task<bool> IsValidFileSizeAsync(long fileSize);
        Task<string> SanitizeFileNameAsync(string fileName);
    }
}