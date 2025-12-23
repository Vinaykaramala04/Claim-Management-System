using ClaimsManagement.Business.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ClaimsManagement.Business.Services
{
    public class DocumentValidationService : IDocumentValidationService
    {
        private readonly ILogger<DocumentValidationService> _logger;
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public DocumentValidationService(ILogger<DocumentValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ValidateDocumentAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File is null or empty");
                    return false;
                }

                var isValidType = await IsValidFileTypeAsync(file.FileName);
                var isValidSize = await IsValidFileSizeAsync(file.Length);

                return isValidType && isValidSize;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating document {FileName}", file?.FileName);
                return false;
            }
        }

        public async Task<bool> IsValidFileTypeAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var isValid = _allowedExtensions.Contains(extension);
            
            if (!isValid)
                _logger.LogWarning("Invalid file type: {Extension} for file {FileName}", extension, fileName);
                
            return await Task.FromResult(isValid);
        }

        public async Task<bool> IsValidFileSizeAsync(long fileSize)
        {
            var isValid = fileSize > 0 && fileSize <= _maxFileSize;
            
            if (!isValid)
                _logger.LogWarning("Invalid file size: {FileSize} bytes (max: {MaxSize})", fileSize, _maxFileSize);
                
            return await Task.FromResult(isValid);
        }

        public async Task<string> SanitizeFileNameAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "document";

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Limit length
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            return await Task.FromResult(sanitized);
        }
    }
}