using Microsoft.AspNetCore.Http;
using ClaimsManagement.Business.DTOs.ClaimDocument;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Interfaces.IRepositories;
using ClaimsManagement.DataAccess.Models;

namespace ClaimsManagement.Business.Services
{
    public class ClaimDocumentService : IClaimDocumentService
    {
        private readonly IClaimDocumentRepository _documentRepository;
        private readonly string _uploadPath = "wwwroot/uploads/documents";

        public ClaimDocumentService(IClaimDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file, int claimId, int uploadedBy)
        {
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new ClaimDocument
            {
                ClaimId = claimId,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            var savedDocument = await _documentRepository.AddAsync(document);
            return MapToResponseDto(savedDocument);
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsByClaimIdAsync(int claimId)
        {
            var documents = await _documentRepository.GetByClaimIdAsync(claimId);
            return documents.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsByUserIdAsync(int userId)
        {
            var documents = await _documentRepository.GetByUserIdAsync(userId);
            return documents.Select(MapToResponseDto);
        }

        public async Task<DocumentResponseDto> GetDocumentByIdAsync(int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            return document != null ? MapToResponseDto(document) : null;
        }

        public async Task DeleteDocumentAsync(int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document != null)
            {
                if (File.Exists(document.FilePath))
                    File.Delete(document.FilePath);
                
                await _documentRepository.DeleteAsync(document);
            }
        }

        public async Task<byte[]> DownloadDocumentAsync(int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || !File.Exists(document.FilePath))
                throw new FileNotFoundException("Document not found");

            return await File.ReadAllBytesAsync(document.FilePath);
        }

        private DocumentResponseDto MapToResponseDto(ClaimDocument document)
        {
            return new DocumentResponseDto
            {
                DocumentId = document.DocumentId,
                ClaimId = document.ClaimId,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedBy = document.UploadedByUser?.FirstName + " " + document.UploadedByUser?.LastName ?? "Unknown",
                UploadedAt = document.UploadedAt
            };
        }
    }
}