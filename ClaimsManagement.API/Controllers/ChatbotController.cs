using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClaimsManagement.Business.Interfaces.IServices;

namespace ClaimsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> ProcessMessage([FromBody] ChatbotRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var response = await _chatbotService.ProcessMessageAsync(request.Message, request.UserRole, userId);
                
                return Ok(new { response });
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

    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
        public int UserRole { get; set; }
        public int UserId { get; set; }
    }
}