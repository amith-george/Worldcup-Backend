using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorldCupPolling.Data;
using WorldCupPolling.DTOs;
using WorldCupPolling.Models;

namespace WorldCupPolling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only authenticated users can vote
    public class VotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VotesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/votes
        [HttpPost]
        public async Task<IActionResult> SubmitVote([FromBody] SubmitVoteDto request)
        {
            // 1. Extract the UserId from the JWT token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            // 2. Check if the user has already voted
            bool hasVoted = await _context.Votes.AnyAsync(v => v.UserId == userId);
            if (hasVoted)
            {
                return BadRequest("You have already cast your vote.");
            }

            // 3. Check if the team exists
            var teamExists = await _context.Teams.AnyAsync(t => t.Id == request.TeamId);
            if (!teamExists)
            {
                return NotFound("The selected team does not exist.");
            }

            // 4. Save the vote
            var vote = new Vote
            {
                UserId = userId,
                TeamId = request.TeamId
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Your vote has been submitted successfully." });
        }

        // GET: api/votes/my-vote
        [HttpGet("my-vote")]
        public async Task<ActionResult<VoteResponseDto>> GetMyVote()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var vote = await _context.Votes
                .Include(v => v.Team)
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vote == null)
            {
                return NotFound("You have not voted yet.");
            }

            return Ok(new VoteResponseDto
            {
                Id = vote.Id,
                UserId = vote.UserId,
                Username = vote.User!.Username,
                TeamId = vote.TeamId,
                TeamName = vote.Team!.TeamName,
                LogoUrl = vote.Team.LogoUrl
            });
        }
    }
}
