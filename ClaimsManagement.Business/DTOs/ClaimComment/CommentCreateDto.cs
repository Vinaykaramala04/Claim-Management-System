using System.ComponentModel.DataAnnotations;

namespace ClaimsManagement.Business.DTOs.ClaimComment
{
    public class CommentCreateDto
    {
        [Required]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;
    }
}