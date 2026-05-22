using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.DTOs
{
    public class AuthDto
    {
    }

    public class UserDto
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
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
        [Required(ErrorMessage = "電子郵件為必填欄位")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填欄位")]
        public string Password { get; set; } = string.Empty;
    }

    


}
