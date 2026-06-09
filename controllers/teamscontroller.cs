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
    [Authorize] // Requires a valid JWT token for all endpoints by default
    public class TeamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/teams
        // Accessible by both Users and Admins to browse teams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamResponseDto>>> GetTeams()
        {
            var teams = await _context.Teams
                .Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    TeamName = t.TeamName,
                    LogoUrl = t.LogoUrl
                })
                .ToListAsync();

            return Ok(teams);
        }

        // 2. GET: api/teams/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TeamResponseDto>> GetTeam(int id)
        {
            var team = await _context.Teams
                .Select(t => new TeamResponseDto
                {
                    Id = t.Id,
                    TeamName = t.TeamName,
                    LogoUrl = t.LogoUrl
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            return Ok(team);
        }

        // 3. POST: api/teams
        // Strict Admin Only verification
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TeamResponseDto>> CreateTeam([FromForm] CreateTeamDto dto)
        {
            // Validation: Ensure team name is unique within the same poll
            if (await _context.Teams.AnyAsync(t => t.TeamName == dto.TeamName && t.PollId == dto.PollId))
            {
                return BadRequest("A team with this name already exists in this poll.");
            }

            var pollExists = await _context.Polls.AnyAsync(p => p.Id == dto.PollId);
            if (!pollExists)
            {
                return BadRequest("The specified poll does not exist.");
            }

            string logoRelativePath = string.Empty;

            if (dto.Logo != null && dto.Logo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "logo");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(dto.Logo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Logo.CopyToAsync(fileStream);
                }

                logoRelativePath = $"/Uploads/logo/{uniqueFileName}";
            }

            var team = new Team
            {
                TeamName = dto.TeamName,
                LogoUrl = logoRelativePath,
                PollId = dto.PollId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var response = new TeamResponseDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                LogoUrl = team.LogoUrl
            };

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, response);
        }

        // 4. PUT: api/teams/{id}
        // Strict Admin Only verification
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamDto dto)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            // Validation: Ensure update doesn't cause a duplicate name with another team
            if (await _context.Teams.AnyAsync(t => t.TeamName == dto.TeamName && t.Id != id && t.PollId == team.PollId))
            {
                return BadRequest("Another team with this name already exists in this poll.");
            }

            team.TeamName = dto.TeamName;
            team.LogoUrl = dto.LogoUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 5. DELETE: api/teams/{id}
        // Strict Admin Only verification
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound($"Team with ID {id} not found.");
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 6. GET: api/teams/results
        // Fetches aggregated vote metrics, restricted by the global reveal switch
        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<TeamResultDto>>> GetResults([FromQuery] int? pollId = null)
        {
            // Check identity properties from the JWT claims to see if requester is an Admin
            bool isAdmin = User.IsInRole("Admin");

            // Fetch the system setting visibility status
            var revealSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "AreResultsRevealed");
            
            bool isRevealed = revealSetting != null && revealSetting.Value.ToLower() == "true";

            // Validation: If results aren't revealed yet, block standard users
            if (!isRevealed && !isAdmin)
            {
                return Forbid("The voting results have not been revealed by the administrator yet.");
            }

            int targetPollId;
            if (pollId.HasValue)
            {
                targetPollId = pollId.Value;
            }
            else
            {
                var activePoll = await _context.Polls.FirstOrDefaultAsync(p => p.IsActive);
                if (activePoll == null) return Ok(new List<TeamResultDto>());
                targetPollId = activePoll.Id;
            }

            // Query logic calculating total votes per team for the specific poll
            var results = await _context.Teams
                .Where(t => t.PollId == targetPollId)
                .Select(t => new TeamResultDto
                {
                    Id = t.Id,
                    TeamName = t.TeamName,
                    LogoUrl = t.LogoUrl,
                    VoteCount = t.Votes.Count() // Count all votes for this team, since Team belongs to this Poll
                })
                .OrderByDescending(r => r.VoteCount)
                .ToListAsync();

            return Ok(results);
        }
    }
}