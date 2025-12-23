using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Enum;
using System.Security.Claims;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PaymentController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public PaymentController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet("approved-claims")]
        public async Task<IActionResult> GetApprovedClaims()
        {
            try
            {
                var approvedClaims = await _claimService.GetClaimsByStatusAsync(ClaimStatus.Approved);
                return Ok(new
                {
                    success = true,
                    count = approvedClaims.Count(),
                    totalAmount = approvedClaims.Sum(c => c.Amount),
                    claims = approvedClaims
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("process/{claimId}")]
        public async Task<IActionResult> ProcessPayment(int claimId, [FromBody] PaymentRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var claim = await _claimService.UpdateClaimStatusAsync(claimId, ClaimStatus.Paid, userId, request.Comments);
                return Ok(new { success = true, message = "Payment processed successfully", claim });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }

    public class PaymentRequest
    {
        public string? Comments { get; set; }
    }
}