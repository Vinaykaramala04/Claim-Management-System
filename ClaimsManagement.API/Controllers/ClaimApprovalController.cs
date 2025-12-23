using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class ClaimApprovalController : ControllerBase
    {
        private readonly IClaimApprovalService _claimApprovalService;

        public ClaimApprovalController(IClaimApprovalService claimApprovalService)
        {
            _claimApprovalService = claimApprovalService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessApproval([FromBody] ApprovalRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var approval = await _claimApprovalService.ProcessApprovalAsync(request.ClaimId, request.Status, userId, request.Comments);
                return Ok(approval);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var userId = GetCurrentUserId();
            var approvals = await _claimApprovalService.GetPendingApprovalsAsync(userId);
            return Ok(approvals);
        }

        [HttpGet("claim/{claimId}")]
        public async Task<IActionResult> GetApprovalsByClaimId(int claimId)
        {
            var approvals = await _claimApprovalService.GetApprovalsByClaimIdAsync(claimId);
            return Ok(approvals);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }

    public class ApprovalRequest
    {
        public int ClaimId { get; set; }
        public ClaimsManagement.DataAccess.Enum.ApprovalStatus Status { get; set; }
        public string? Comments { get; set; }
    }
}