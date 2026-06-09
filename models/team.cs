using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        // New field to store the image path or URL
        [StringLength(255)]
        public string LogoUrl { get; set; } = string.Empty;

        [Required]
        public int PollId { get; set; }

        public virtual Poll? Poll { get; set; }

        // Navigation property for votes cast for this team
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}