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

            // 2. Check if the team exists to get its PollId
            var team = await _context.Teams.Include(t => t.Poll).FirstOrDefaultAsync(t => t.Id == request.TeamId);
            if (team == null)
            {
                return NotFound("The selected team does not exist.");
            }

            // 3. Check if the poll is active
            if (team.Poll == null || !team.Poll.IsActive)
            {
                return BadRequest("The poll for this team is not currently active.");
            }

            // 4. Check if the user has already voted in this specific poll
            bool hasVoted = await _context.Votes.AnyAsync(v => v.UserId == userId && v.PollId == team.PollId);
            if (hasVoted)
            {
                return BadRequest("You have already cast your vote in this poll.");
            }

            // 5. Save the vote
            var vote = new Vote
            {
                UserId = userId,
                TeamId = request.TeamId,
                PollId = team.PollId
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Your vote has been submitted successfully." });
        }

        // GET: api/votes/my-vote/{pollId}
        [HttpGet("my-vote/{pollId:int}")]
        public async Task<ActionResult<VoteResponseDto>> GetMyVote(int pollId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var pollExists = await _context.Polls.AnyAsync(p => p.Id == pollId);
            if (!pollExists)
            {
                return NotFound("The specified poll does not exist.");
            }

            var vote = await _context.Votes
                .Include(v => v.Team)
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == userId && v.PollId == pollId);

            if (vote == null)
            {
                return NotFound("You have not voted yet in this poll.");
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
