using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using IeltsTestWeb.RequestModels;
using MailKit.Net.Smtp;
using StackExchange.Redis;
using IeltsTestWeb.Utils;

namespace IeltsTestWeb.Controllers
{
    [Route("auth")]
    [ApiController]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ieltsDbContext database;
        private readonly IConfiguration configuration;
        private readonly IConnectionMultiplexer redis;
        private readonly JwtUtil jwtUtil;
        private int verifyMinute { get; set; } = 5;
        public AuthenticationController(ieltsDbContext database, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            this.database = database;
            this.configuration = configuration;
            this.redis = redis;

            jwtUtil = new JwtUtil(configuration);
        }

        /// <summary>
        /// Get all roles in the system.
        /// </summary>
        [HttpGet("role")]
        public async Task<ActionResult<IEnumerable<Models.Role>>> GetAllRoles()
        {
            var roleList = await database.Roles.ToListAsync();
            return Ok(roleList);
        }

        /// <summary>
        /// Get role by id.
        /// </summary>
        [HttpPatch("role/{id}")]
        public async Task<IActionResult> UpdateAccountRole(int id, [FromBody] int roleId)
        {
            var account = await database.Accounts.FindAsync(id);
            
            if (account == null)
                return NotFound("Can't find account with id " + id);

            if (database.Roles.Any(role => role.RoleId == roleId))
            {
                account.RoleId = roleId;
                await database.SaveChangesAsync();
                return Ok("Update account's role successfully");
            }

            return NotFound("Can't find role with id " + roleId);
        }

        /// <summary>
        /// Login to the system.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = await database.Accounts.SingleOrDefaultAsync(account => account.Email.Equals(request.Email));

            if (account != null && BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
            {
                var role = await database.Roles.FindAsync(account.RoleId);
                if (role != null)
                {
                    var accessToken = jwtUtil.GenerateAccessToken(account.AccountId, account.Email, role.Name);
                    var refreshToken = jwtUtil.GenerateRefreshToken(account.AccountId);
                    return Ok(new {accessToken = accessToken, refreshToken = refreshToken});
                }
                return BadRequest("Can't find user's role");
            }

            return Unauthorized("Invalid username or password.");
        }

        /// <summary>
        /// Test the authentication for the Admin role.
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpGet("test")]
        public IActionResult TestAuthentication()
        {
            return Ok("This is ADMIN DATA");
        }

        /// <summary>
        /// Send verification code to a specific email.
        /// </summary>
        [HttpPost("code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email)
        {
            // Generate verification code
            var verificationCode = new Random().Next(1000, 9999).ToString();

            // Store verification code in Redis database
            var redisDb = redis.GetDatabase();
            await redisDb.StringSetAsync(email, verificationCode, TimeSpan.FromMinutes(verifyMinute));

            // Create a message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ieltsTest", configuration["EmailAuthen:Email"]));
            message.To.Add(new MailboxAddress("User", email));
            message.Subject = "Your verification code";
            message.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {verificationCode}"
            };

            // Send Verification code to user
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(configuration["EmailAuthen:Email"], configuration["EmailAuthen:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok("Verification code: " + verificationCode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Verify the account.
        /// </summary>
        [HttpPost("verify")]
        public async Task<IActionResult> Verification([FromBody] VerifyRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var redisDb = redis.GetDatabase();
            string? storedCode = await redisDb.StringGetAsync(request.Email);

            if (storedCode != null && storedCode.Equals(request.VerificationCode))
            {
                await redisDb.KeyDeleteAsync(request.Email);
                return Ok("Verification successful.");
            }

            return BadRequest("Invalid verification code.");
        }

        /// <summary>
        /// Reset password.
        /// </summary>
        [HttpPost("password")]
        public async Task<IActionResult> ResetPassword([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedAccount = await database.Accounts.SingleOrDefaultAsync(ac => ac.Email.Equals(request.Email));

            if (updatedAccount != null)
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                updatedAccount.Password = hashedPassword;
                await database.SaveChangesAsync();
                return Ok("Password changes to: " + request.Password);
            }

            return NotFound("Can't find account.");
        }
    }
}
