using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.DTOs.Claim;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromBody] ClaimCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var claim = await _claimService.CreateClaimAsync(request, userId);
                return CreatedAtAction(nameof(GetClaim), new { id = claim.ClaimId }, claim);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaim(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
                return NotFound(new { message = "Claim not found" });

            return Ok(claim);
        }

        [HttpGet("number/{claimNumber}")]
        public async Task<IActionResult> GetClaimByNumber(string claimNumber)
        {
            var claim = await _claimService.GetClaimByNumberAsync(claimNumber);
            if (claim == null)
                return NotFound(new { message = "Claim not found" });

            return Ok(claim);
        }

        [HttpGet("my-claims")]
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = GetCurrentUserId();
            var claims = await _claimService.GetClaimsByUserAsync(userId);
            return Ok(claims);
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Agent,Manager,Admin")]
        public async Task<IActionResult> GetClaimsByStatus(ClaimStatus status)
        {
            var claims = await _claimService.GetClaimsByStatusAsync(status);
            return Ok(claims);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Agent,Manager,Admin")]
        public async Task<IActionResult> UpdateClaimStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                
                if (request.Status == ClaimStatus.Approved && userRole != "Manager" && userRole != "Admin")
                {
                    return Forbid("Only Managers and Admins can approve claims");
                }
                
                if (request.Status == ClaimStatus.Paid && userRole != "Admin")
                {
                    return Forbid("Only Admins can process payments");
                }
                
                var claim = await _claimService.UpdateClaimStatusAsync(id, request.Status, userId, request.Comments);
                return Ok(claim);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("pending-approvals")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var userId = GetCurrentUserId();
            var claims = await _claimService.GetPendingApprovalsAsync(userId);
            return Ok(claims);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }

    public class UpdateStatusRequest
    {
        public ClaimStatus Status { get; set; }
        public string? Comments { get; set; }
    }
}