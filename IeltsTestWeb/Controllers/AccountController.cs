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
        private readonly string imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Avatars");
        public AccountController(ieltsDbContext database)
        {
            this.database = database;

            //Ensure the directory exist
            Directory.CreateDirectory(imageUploadPath);
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

        [HttpPatch("DeactivateAccount/{id}")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            var account = await database.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Can't find account with id " + id);
            account.IsActive = false;
            await database.SaveChangesAsync();
            return Ok("Deactivate account successfully!");
        }
        
        //[HttpPost("UpdateProfileImage/{id}")]
        //public async Task<IActionResult> UpdateProfileImage(int id, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    var account = await database.Accounts.FindAsync(id);
        //    if (account == null)
        //        return NotFound("Can't find account with id " + id);

        //    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        //    var extension = Path.GetExtension(file.FileName).ToLower();
        //    var savedFilePath = Path.Combine(imageUploadPath, fileName + extension);

        //    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        //        return BadRequest("Invalid file type. Only JPG, JPEG, PNG are allowed.");

        //    // Delete old image
        //    if (!string.IsNullOrEmpty(account.AvatarLink) && System.IO.File.Exists(account.AvatarLink))
        //        System.IO.File.Delete(account.AvatarLink);

        //    // Đặt tên tệp và đường dẫn
        //    var fileExtension = Path.GetExtension(avatar.FileName);
        //    var fileName = $"avatar_{id}{fileExtension}";
        //    var filePath = Path.Combine("uploads", "avatars", fileName);

        //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //    // Tối ưu hóa và lưu ảnh mới
        //    using (var image = Image.Load(avatar.OpenReadStream()))
        //    {
        //        // Giảm kích thước ảnh nếu lớn hơn 500x500
        //        int maxWidth = 500;
        //        int maxHeight = 500;

        //        if (image.Width > maxWidth || image.Height > maxHeight)
        //        {
        //            image.Mutate(x => x.Resize(new ResizeOptions
        //            {
        //                Mode = ResizeMode.Max,
        //                Size = new Size(maxWidth, maxHeight)
        //            }));
        //        }

        //        // Thiết lập chất lượng nén ảnh
        //        var encoder = new JpegEncoder
        //        {
        //            Quality = 75 // Đặt chất lượng nén (1-100)
        //        };

        //        // Lưu ảnh đã tối ưu hóa
        //        await image.SaveAsync(filePath, encoder);
        //    }

        //    // Cập nhật đường dẫn ảnh trong cơ sở dữ liệu và tạo URL
        //    account.AvatarLink = $"/uploads/avatars/{fileName}";
        //    await _context.SaveChangesAsync();

        //    var avatarUrl = $"{Request.Scheme}://{Request.Host}{account.AvatarLink}";
        //    return Ok(new { AvatarUrl = avatarUrl });
        //}

    }
}
