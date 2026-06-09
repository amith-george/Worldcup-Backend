using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCupPolling.Data;
using WorldCupPolling.DTOs;
using WorldCupPolling.Models;
using WorldCupPolling.Services;

namespace WorldCupPolling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        // Inject the database context and your custom JWT Service
        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Check if the user already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            // 2. Create the user and hash their password securely using BCrypt
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User" // Every standard registration defaults to "User"
            };

            // 3. Save to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Find the user by username
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            // 2. If user doesn't exist OR the password doesn't match the hash, reject them
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // 3. Generate the JWT token using your service
            var token = _jwtService.GenerateToken(user);

            // 4. Return the token and user details to the Angular frontend
            return Ok(new 
            { 
                Token = token, 
                Username = user.Username, 
                Role = user.Role 
            });
        }

        // -------------------------------------------------------------------
        // DEVELOPMENT HELPER: Endpoint to easily create your initial Admin
        // -------------------------------------------------------------------
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromQuery] string secretKey, [FromBody] RegisterDto request)
        {
            var expectedSecret = Environment.GetEnvironmentVariable("ADMIN_REGISTRATION_KEY");
            if (string.IsNullOrEmpty(expectedSecret) || secretKey != expectedSecret)
            {
                return Unauthorized("Invalid or missing secret registration key.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            var adminUser = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Admin" // Hardcoded to Admin
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin account created successfully." });
        }
    }
}