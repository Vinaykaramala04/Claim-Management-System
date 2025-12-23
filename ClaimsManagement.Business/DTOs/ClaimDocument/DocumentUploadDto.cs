using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClaimsManagement.Business.DTOs.ClaimDocument
{
    public class DocumentUploadDto
    {
        [Required]
        public int ClaimId { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}