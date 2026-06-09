using System.ComponentModel.DataAnnotations;

namespace WorldCupPolling.DTOs
{
    public class UpdateSystemSettingDto
    {
        [Required(ErrorMessage = "A value must be provided.")]
        public string Value { get; set; } = string.Empty;
    }
}