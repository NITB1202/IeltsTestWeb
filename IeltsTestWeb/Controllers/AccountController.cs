﻿using IeltsTestWeb.Models;
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
using MailKit.Net.Smtp;
using System.Security.Principal;
using StackExchange.Redis;

namespace IeltsTestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ieltsDbContext database;
        private readonly IConfiguration configuration;
        private readonly IConnectionMultiplexer redis;
        public AccountController(ieltsDbContext database, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            this.database = database;
            this.configuration = configuration;
            this.redis = redis;
        }

        private int verifyMinute { get; set; } = 5;

        [HttpGet("AllRoles")]
        public async Task<ActionResult<IEnumerable<Models.Role>>> GetAllRoles()
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

        [HttpPost("ValidateNewAccount")]
        public IActionResult ValidateNewAccount([FromBody] AccountRequestModel request)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Check format
            if (!Regex.IsMatch(request.Email, emailPattern))
                return BadRequest("Invalid email");

            // Check duplicate
            if (!ModelState.IsValid)
                return BadRequest("The email used for registration already exists.");

            // --Validate password if necessary--

            //Check valid role
            if (database.Roles.Any(role => role.RoleId.Equals(request.RoleId)))
                return Ok("Valid email");

            return BadRequest("Invalid role id");

        }

        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(string email)
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

                return Ok("Verification code:" + verificationCode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Verification")]
        public async Task<IActionResult> Verification([FromBody] VerifyRequestModel request)
        {
            var redisDb = redis.GetDatabase();
            string? storedCode = await redisDb.StringGetAsync(request.Email);

            if (storedCode != null && storedCode.Equals(request.VerificationCode))
            {
                await redisDb.KeyDeleteAsync(request.Email);
                return Ok("Verification successful.");
            }

            return BadRequest("Invalid verification code.");
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Account>> CreateNewAccount([FromBody] Account account)
        {
            account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
            database.Accounts.Add(account);
            await database.SaveChangesAsync();
            return Ok(account);
        }
    }
}
