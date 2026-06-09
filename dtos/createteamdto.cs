using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.DTOs
{
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 100 characters.")]
        public string TeamName { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please provide a valid URL for the team logo.")]
        [StringLength(255, ErrorMessage = "Logo URL cannot exceed 255 characters.")]
        public string LogoUrl { get; set; } = string.Empty;
    }
}