using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.Models
{
    public class SystemSetting
    {
        [Key]
        public string Key { get; set; } = string.Empty; // e.g., "AreResultsRevealed"

        [Required]
        public string Value { get; set; } = string.Empty; // "true" or "false"
    }
}