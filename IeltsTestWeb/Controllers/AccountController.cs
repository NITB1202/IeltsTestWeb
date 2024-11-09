using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public AccountController(ieltsDbContext database)
        {
            this.database = database;
        }
        private AccountResponseModel AccountToResponseModel(Account account)
        {
            return new AccountResponseModel
            {
                AccountId = account.AccountId,
                Email = account.Email,
                RoleId = account.RoleId,
                AvatarLink = account.AvatarLink,
                IsActive = account.IsActive
            };
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AccountResponseModel>>> GetAllAcounts()
        {
            var accounts = await database.Accounts.ToListAsync();
            var responseList = accounts.Select(account => AccountToResponseModel(account)).ToList();
            return Ok(responseList);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<AccountRequestModel>> CreateNewAccount([FromBody] AccountRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var account = new Account
            {
                Email = request.Email,
                Password = hashPassword,
                RoleId = request.RoleId
            };

            database.Accounts.Add(account);
            await database.SaveChangesAsync();
            return Ok(account);
        }
        
        [HttpGet("FindById")]
        public async Task<ActionResult<AccountResponseModel>> FindAccountById([FromHeader] int id)
        {
            var account = await database.Accounts.FindAsync(id);

            if(account == null)
                return NotFound("Can't find account with id "+ id);

            return Ok(AccountToResponseModel(account));
        }

        [HttpGet("FindMatch")]
        public ActionResult<IEnumerable<AccountResponseModel>> FindAccountsMatch([FromBody] QueryAccountRequestModel request)
        {
            var accounts = database.Accounts.Where(account =>
                (request.Email == null || account.Email.StartsWith(request.Email)) &&
                (request.RoleId == null || account.RoleId == request.RoleId) &&
                (!request.IsActive.HasValue || account.IsActive == request.IsActive)
            );
            var responseList = accounts.Select(account => AccountToResponseModel(account)).ToList();
            return Ok(responseList);
        }

        [HttpPatch("Deactivate/{id}")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            var account = await database.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Can't find account with id " + id);
            account.IsActive = false;
            await database.SaveChangesAsync();
            return Ok("Deactivate account successfully!");
        }

        [HttpPost("UpdateProfileImage/{id}")]
        public async Task<IActionResult> UpdateProfileImage(int id, IFormFile file)
        {
            var account = await database.Accounts.FindAsync(id);

            if (account == null)
                return NotFound("Can't find account with id " + id);

            if (!ResourcesManager.IsImageValid(file))
                return BadRequest("Invalid image file");

            // Delete old image
            ResourcesManager.RemoveImage(account.AvatarLink);

            // Ensure create avatars directory
            Directory.CreateDirectory(ResourcesManager.avatarsDir);

            // Create file path
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"avatar_{id}{fileExtension}";
            var filePath = Path.Combine(ResourcesManager.avatarsDir,fileName);

            // Save image
            await ResourcesManager.SaveImage(file, filePath);

            // Save image url
            var relativePath = ResourcesManager.GetRelativePath(filePath);
            account.AvatarLink = relativePath;
            await database.SaveChangesAsync();

            var avatarUrl = $"{Request.Scheme}://{Request.Host}{account.AvatarLink}";
            return Ok(new { AvatarUrl = avatarUrl });
        }

        [HttpPatch("Update/{id}")]
        public async Task<ActionResult<AccountResponseModel>> UpdateAccount(int id, [FromBody] UpdateAccountRequestModel request)
        {
            var account = await database.Accounts.FindAsync(id);

            if (account == null)
                return NotFound("Can't find account with id " + id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if(!string.IsNullOrEmpty(request.Email))
                account.Email = request.Email;

            if (request.Goal != null)
                account.Goal = request.Goal;

            await database.SaveChangesAsync();

            return Ok(AccountToResponseModel(account));
        }
    }
}
