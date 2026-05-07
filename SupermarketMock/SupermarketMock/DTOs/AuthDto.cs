using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.DTOs
{
    public class AuthDto
    {
    }

    public class UserDto
    {
        public int userId { get; set; }

        [Required]
        [StringLength(50)]
        public string username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

    }

    public class UserRegisterDto
    {
        [Required]
        [StringLength(50)]
        public string username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string password { get; set; } = string.Empty;
    }
}
