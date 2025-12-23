using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimDocumentController : ControllerBase
    {
        private readonly IClaimDocumentService _claimDocumentService;

        public ClaimDocumentController(IClaimDocumentService claimDocumentService)
        {
            _claimDocumentService = claimDocumentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file, int claimId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            try
            {
                var userId = GetCurrentUserId();
                var document = await _claimDocumentService.UploadDocumentAsync(file, claimId, userId);
                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("claim/{claimId}")]
        public async Task<IActionResult> GetDocumentsByClaimId(int claimId)
        {
            var documents = await _claimDocumentService.GetDocumentsByClaimIdAsync(claimId);
            return Ok(documents);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var document = await _claimDocumentService.GetDocumentByIdAsync(id);
                if (document == null)
                    return NotFound("Document not found");

                var fileBytes = await _claimDocumentService.DownloadDocumentAsync(id);
                return File(fileBytes, document.ContentType ?? "application/octet-stream", document.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                await _claimDocumentService.DeleteDocumentAsync(id);
                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }
}