using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCupPolling.Data;
using WorldCupPolling.DTOs;
using WorldCupPolling.Models;

namespace WorldCupPolling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require login by default
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/settings
        // Admin-only: View all system configurations
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SystemSettingResponseDto>>> GetAllSettings()
        {
            var settings = await _context.SystemSettings
                .Select(s => new SystemSettingResponseDto
                {
                    Key = s.Key,
                    Value = s.Value
                })
                .ToListAsync();

            return Ok(settings);
        }

        // 2. GET: api/settings/{key}
        // Accessible by standard Users so Angular knows how to render the UI
        [HttpGet("{key}")]
        public async Task<ActionResult<SystemSettingResponseDto>> GetSetting(string key)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower());

            if (setting == null)
            {
                return NotFound($"Setting with key '{key}' not found.");
            }

            return Ok(new SystemSettingResponseDto
            {
                Key = setting.Key,
                Value = setting.Value
            });
        }

        // 3. PUT: api/settings/{key}
        // Admin-only: Update a specific configuration value
        [HttpPut("{key}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSystemSettingDto dto)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower());

            // If the setting doesn't exist, create it dynamically
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    Value = dto.Value
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = dto.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Setting '{setting.Key}' updated successfully.", newValue = setting.Value });
        }
    }
}