using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClaimsManagement.Business.Interfaces.IServices;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public TestController(IClaimService claimService)
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
                    count = approvedClaims.Count(),
                    claims = approvedClaims,
                    statusValue = (int)ClaimStatus.Approved
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("all-claims-status")]
        public async Task<IActionResult> GetAllClaimsWithStatus()
        {
            try
            {
                var allStatuses = new[]
                {
                    ClaimStatus.Draft,
                    ClaimStatus.Submitted,
                    ClaimStatus.UnderReview,
                    ClaimStatus.MoreInfoRequired,
                    ClaimStatus.Approved,
                    ClaimStatus.Rejected,
                    ClaimStatus.Paid,
                    ClaimStatus.Cancelled
                };

                var result = new List<object>();
                
                foreach (var status in allStatuses)
                {
                    var claims = await _claimService.GetClaimsByStatusAsync(status);
                    result.Add(new
                    {
                        status = status.ToString(),
                        statusValue = (int)status,
                        count = claims.Count(),
                        claims = claims.Select(c => new { c.ClaimId, c.ClaimNumber, c.Status, c.Title })
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}