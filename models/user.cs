using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User"; // "Admin" or "User"

        // Navigation property for the user's vote
        public virtual Vote? Vote { get; set; }
    }
}