using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
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

        [HttpGet("GetAllAccount")]
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
        
        [HttpGet("FindAccountById")]
        public async Task<ActionResult<AccountResponseModel>> FindAccountById([FromHeader] int id)
        {
            var account = await database.Accounts.FindAsync(id);

            if(account == null)
                return NotFound("Can't find account with id "+ id);

            return Ok(AccountToResponseModel(account));
        }

        [HttpGet("FindAccountsMatch")]
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

        [HttpPatch("UpdateProfileImage/{id}")]
        public async Task<ActionResult<string>> UpdateProfileImage(int id, [FromBody] string img)
        {
            return Ok();
        }


    }
}
