using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.DTOs
{
    public class UpdateProfileDto
    {
        [StringLength(50)]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
