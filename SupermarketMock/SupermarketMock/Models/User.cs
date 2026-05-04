using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.Models
{
    public class User
    {
        public int Id { get; set; }


        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public Cart? Cart { get; set; }

    }
}
