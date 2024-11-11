using IeltsTestWeb.Models;
using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SoundController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public SoundController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Upload an audio file for a listening test.
        /// </summary>
        [HttpPost("{testId}")]
        public async Task<ActionResult<Sound>> UploadTestSound(int testId, IFormFile file)
        {
            var test = await database.Tests.FindAsync(testId);

            if (test == null)
                return NotFound("Can't find test with id " + testId);

            if (test.TestSkill != "listening")
                return BadRequest("The test skill is not 'listening'");

            if (!ResourcesManager.IsSoundValid(file))
                return BadRequest("Invalid sound");

            // Delete old sound
            var oldSound = await database.Sounds.FirstOrDefaultAsync(sound => sound.TestId == testId);
            if (oldSound != null)
                ResourcesManager.RemoveFile(oldSound.SoundLink);

            // Ensure create sound directory
            Directory.CreateDirectory(ResourcesManager.soundsDir);

            // Create file path
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"soundTest_{testId}{fileExtension}";
            var filePath = Path.Combine(ResourcesManager.soundsDir, fileName);

            // Save sound
            await ResourcesManager.SaveSound(file, filePath);

            // Save sound url
            var relativePath = ResourcesManager.GetRelativePath(filePath);
            if (oldSound == null)
            {
                var sound = new Sound
                {
                    TestId = testId,
                    SoundLink = relativePath
                };

                database.Sounds.Add(sound);
            }
            else
                oldSound.SoundLink = relativePath;

            await database.SaveChangesAsync();

            var soundUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
            return Ok(new { soundUrl = soundUrl });
        }

        /// <summary>
        /// Get a listening test audio file.
        /// </summary>
        [HttpGet("{testId}")]
        public async Task<IActionResult> GetTestSound(int testId)
        {
            var sound = await database.Sounds.FirstOrDefaultAsync(s => s.TestId == testId);

            if (sound == null)
                return NotFound("Can't find sound for test with " + testId);

            return Ok(sound.SoundLink);
        }
    }
}
