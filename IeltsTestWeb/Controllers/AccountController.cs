using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace IeltsTestWeb.Controllers
{
    [Route("account")]
    [ApiController]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public AccountController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Get all accounts in the system.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountResponseModel>>> GetAllAcounts()
        {
            var accounts = await database.Accounts.ToListAsync();
            var responseList = accounts.Select(account => Mapper.AccountToResponseModel(account)).ToList();
            return Ok(responseList);
        }

        /// <summary>
        /// Create new account.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AccountResponseModel>> CreateNewAccount([FromBody] AccountRequestModel request)
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
            return Ok(Mapper.AccountToResponseModel(account));
        }

        /// <summary>
        /// Get account by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountResponseModel>> FindAccountById(int id)
        {
            var account = await database.Accounts.FindAsync(id);

            if(account == null)
                return NotFound("Can't find account with id "+ id);

            return Ok(Mapper.AccountToResponseModel(account));
        }

        /// <summary>
        /// Find all accounts that match the query parameters.
        /// </summary>
        [HttpGet("match")]
        public ActionResult<IEnumerable<AccountResponseModel>> FindAccountsMatch(
            [FromQuery] string? email, [FromQuery] int? roleId, [FromQuery] bool? isActive)
        {
            var accounts = database.Accounts.Where(account =>
                (email == null || account.Email.ToLower().StartsWith(email.ToLower())) &&
                (roleId == null || account.RoleId == roleId) &&
                (!isActive.HasValue || account.IsActive == isActive)
            );
            var responseList = accounts.Select(account => Mapper.AccountToResponseModel(account)).ToList();
            return Ok(responseList);
        }

        /// <summary>
        /// Deactivate a specific account.
        /// </summary>
        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            var account = await database.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Can't find account with id " + id);
            account.IsActive = false;
            await database.SaveChangesAsync();
            return Ok("Deactivate account successfully!");
        }

        /// <summary>
        /// Upload a profile picture for an account.
        /// </summary>
        [HttpPost("image/{id}")]
        public async Task<IActionResult> UpdateProfileImage(int id, IFormFile file)
        {
            var account = await database.Accounts.FindAsync(id);

            if (account == null)
                return NotFound("Can't find account with id " + id);

            if (!ResourcesManager.IsImageValid(file))
                return BadRequest("Invalid image file");

            // Delete old image
            ResourcesManager.RemoveFile(account.AvatarLink);

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

        /// <summary>
        /// Update the account information.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<AccountResponseModel>> UpdateAccount(int id, [FromBody] UpdateAccountRequestModel request)
        {
            var account = await database.Accounts.FindAsync(id);

            if (account == null)
                return NotFound("Can't find account with id " + id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestProperties = typeof(UpdateAccountRequestModel).GetProperties();
            var accountProperties = typeof(Account).GetProperties();

            foreach (var prop in requestProperties)
            {
                var requestValue = prop.GetValue(request);

                // If the value is not null, find corresponding property in account and update
                if (requestValue != null)
                {
                    var accountProp = accountProperties.FirstOrDefault(p => p.Name == prop.Name);
                    if (accountProp != null && accountProp.CanWrite)
                        accountProp.SetValue(account, requestValue);
                }
            }

            await database.SaveChangesAsync();

            return Ok(Mapper.AccountToResponseModel(account));
        }

        /// <summary>
        /// Get account profile picture.
        /// </summary>
        [HttpGet("image/{id}")]
        public async Task<ActionResult<string>> GetUserAvatar(int id)
        {
            var account = await database.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Can't find account with id " + id);
            return Ok(account.AvatarLink);
        }
    }
}
