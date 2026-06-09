namespace WorldCupPolling.DTOs
{
    public class VoteResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;
        
        public string LogoUrl { get; set; } = string.Empty;
    }
}