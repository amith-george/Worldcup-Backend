using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCupPolling.Data;
using WorldCupPolling.Models;

namespace WorldCupPolling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PollsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PollsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/polls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Poll>>> GetPolls()
        {
            return await _context.Polls.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public class CreatePollDto
        {
            public string Title { get; set; } = string.Empty;
        }

        // POST: api/polls
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Poll>> CreatePoll([FromBody] CreatePollDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Poll title is required.");
            }

            // Set all existing polls to inactive
            var activePolls = await _context.Polls.Where(p => p.IsActive).ToListAsync();
            foreach (var p in activePolls)
            {
                p.IsActive = false;
            }

            var newPoll = new Poll
            {
                Title = dto.Title,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Polls.Add(newPoll);
            await _context.SaveChangesAsync();

            return Ok(newPoll);
        }
    }
}
