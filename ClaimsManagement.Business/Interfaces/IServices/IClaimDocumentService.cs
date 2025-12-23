using ClaimsManagement.Business.DTOs.ClaimDocument;
using Microsoft.AspNetCore.Http;

namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IClaimDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file, int claimId, int uploadedBy);
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsByClaimIdAsync(int claimId);
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsByUserIdAsync(int userId);
        Task<DocumentResponseDto> GetDocumentByIdAsync(int documentId);
        Task DeleteDocumentAsync(int documentId);
        Task<byte[]> DownloadDocumentAsync(int documentId);
    }
}