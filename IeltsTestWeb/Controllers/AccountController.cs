using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;
using MimeKit;
using IeltsTestWeb.RequestModels;

namespace IeltsTestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ieltsDbContext database;
        private readonly IConfiguration configuration;
        private int verifyMinute { get; set; } = 5;
        public AccountController(ieltsDbContext database, IConfiguration configuration)
        {
            this.database = database;
            this.configuration = configuration;
        }

        [HttpGet("AllRoles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            var roleList = await database.Roles.ToListAsync();
            return Ok(roleList);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            var account = await database.Accounts.SingleOrDefaultAsync(account => account.Email.Equals(request.Email));

            if (account != null && BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
            {
                var role = await database.Roles.FindAsync(account.RoleId);
                var token = GenerateJwtToken(account.Email, role.Name);
                return Ok(token);
            }

            return Unauthorized("Invalid username or password.");
        }

        [Authorize(Roles = "admin")]
        [HttpGet("TestAuthentication")]
        public IActionResult TestAuthentication()
        {
            return Ok("This is ADMIN DATA");
        }

        private string GenerateJwtToken(string username, string role)
        {
            // Create basic claims 
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add claim for role
            claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Account>> CreateNewAccount([FromBody] Account account)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Check format
            if (!Regex.IsMatch(account.Email, emailPattern))
                return BadRequest("Invalid email");

            // Check duplicate
            if (!ModelState.IsValid)
                return BadRequest("The email used for registration already exists.");

            // Generate verification code
            var verificationCode = new Random().Next(1000, 9999).ToString();
            //await _verificationCodeService.SetVerificationCodeAsync(account.Email, verificationCode, TimeSpan.FromMinutes(verifyMinute));

            // Send Verification code to email

            var message = new MimeMessage();
            //message.From(new MailboxAddress("ieltsTest", configuration["Email:"]))
           
            // Create new account
            account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
            database.Accounts.Add(account);
            await database.SaveChangesAsync();
            return Ok(account);
        }



    }
}
