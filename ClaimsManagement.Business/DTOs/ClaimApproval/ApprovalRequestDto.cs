using System.ComponentModel.DataAnnotations;
using ClaimsManagement.DataAccess.Enum;

namespace ClaimsManagement.Business.DTOs.ClaimApproval
{
    public class ApprovalRequestDto
    {
        [Required]
        public int ClaimId { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public int ApprovalLevel { get; set; } = 1;
    }
}