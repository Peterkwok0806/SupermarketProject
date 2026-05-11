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

        public string role { get; set; } = string.Empty;
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

    public class LoginDto
    {
        [Required]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "Supermarket";
        public string Audience { get; set; } = "SupermarketClient";
        public int ExpiresInMinutes { get; set; } = 60 * 24; // 24小時
    }


}
