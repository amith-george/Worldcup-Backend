namespace WorldCupPolling.DTOs
{
    public class TeamResultDto
    {
        public int Id { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public int VoteCount { get; set; }
    }
}