using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.DTOs
{
    public class SubmitVoteDto
    {
        [Required(ErrorMessage = "Team selection is required.")]
        public int TeamId { get; set; }
    }
}