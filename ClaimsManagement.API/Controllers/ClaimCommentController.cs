using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimCommentController : ControllerBase
    {
        private readonly IClaimCommentService _claimCommentService;

        public ClaimCommentController(IClaimCommentService claimCommentService)
        {
            _claimCommentService = claimCommentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var comment = await _claimCommentService.AddCommentAsync(request.ClaimId, request.Comment, userId, request.IsInternal);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("claim/{claimId}")]
        public async Task<IActionResult> GetCommentsByClaimId(int claimId)
        {
            var comments = await _claimCommentService.GetCommentsByClaimIdAsync(claimId);
            return Ok(comments);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _claimCommentService.DeleteCommentAsync(id);
                return Ok(new { message = "Comment deleted successfully" });
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

    public class CommentRequest
    {
        public int ClaimId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false;
    }
}