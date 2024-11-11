using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
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
    public class SectionController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public SectionController(ieltsDbContext database)
        {
            this.database = database;
        }
        private static ReadingSectionResponseModel ReadingSectionToResponseModel(ReadingSection model)
        {
            return new ReadingSectionResponseModel
            {
                Id = model.RsectionId,
                ImageLink = model.ImageLink,
                Title = model.Title,
                Content = model.Content,
                TestId = model.TestId
            };
        }
        private static ListeningSectionResponseModel ListeningSectionToResponseModel(ListeningSection model)
        {
            return new ListeningSectionResponseModel
            {
                Id = model.LsectionId,
                SectionOrder = model.SectionOrder,
                TimeStamp = model.TimeStamp,
                Transcript = model.Transcript,
                SoundId = model.SoundId
            };
        }

        /// <summary>
        /// Create new reading section.
        /// </summary>
        [HttpPost("Reading/{testId}")]
        public async Task<ActionResult<ReadingSectionResponseModel>> CreateReadingSection(int testId, [FromBody] ReadingSectionRequestModel request)
        {
            var test = await database.Tests.FindAsync(testId);

            if (test == null)
                return NotFound("Can't find test with id " + testId);

            if (test.TestSkill != "reading")
                return BadRequest("Can't add a reading section to a listening test");

            var section = new ReadingSection
            {
                Title = request.Title,
                Content = request.Content,
                TestId = testId
            };

            database.ReadingSections.Add(section);
            await database.SaveChangesAsync();

            return Ok(ReadingSectionToResponseModel(section));
        }

        /// <summary>
        /// Create new listening section.
        /// </summary>
        [HttpPost("Listening/{testId}")]
        public async Task<ActionResult<ListeningSectionResponseModel>> CreateListeningSection(int testId, [FromBody] ListeningSectionRequestModel request)
        {
            // Validate data
            var test = await database.Tests.FindAsync(testId);

            if (test == null)
                return NotFound("Can't find test with id " + testId);

            if (test.TestSkill != "listening")
                return BadRequest("Can't add a listening section to a reading test");

            if (!await database.Sounds.AnyAsync(sound => sound.SoundId == request.SoundId))
                return NotFound("Can't find sound with id ");

            // Check dupplicate
            if (await database.ListeningSections.AnyAsync(section =>
                section.SoundId == request.SoundId && section.SectionOrder == request.SectionOrder))
                return BadRequest("This sound already has section " + request.SectionOrder);

            var section = new ListeningSection
            {
                SectionOrder  = request.SectionOrder,
                TimeStamp = request.TimeStamp,
                Transcript = request.Transcript,
                SoundId = request.SoundId
            };

            database.ListeningSections.Add(section);
            await database.SaveChangesAsync();

            return Ok(ListeningSectionToResponseModel(section));
        }

        /// <summary>
        /// Get all sections belong to the test.
        /// </summary>
        [HttpGet("{testId}")]
        public async Task<IActionResult> FindAllTestSections(int testId)
        {
            var test = await database.Tests.FindAsync(testId);

            if (test == null)
                return NotFound("Can't find test with id " + testId);

            if(test.TestSkill == "reading")
            {
                var sections = database.ReadingSections.Where(s => s.TestId == testId);
                var responseList = sections.Select(s => ReadingSectionToResponseModel(s));
                return Ok(responseList);
            }

            if(test.TestSkill == "listening")
            {
                var soundId = await database.Sounds
                    .Where(sound => sound.TestId == testId)
                    .Select(sound => sound.SoundId)
                    .FirstOrDefaultAsync();

                var sections = database.ListeningSections.Where(s => s.SoundId == soundId);
                var responseList = sections.Select(s => ListeningSectionToResponseModel(s));
                return Ok(responseList);
            }

            return NoContent();
        }

        /// <summary>
        /// Update the reading section information.
        /// </summary>
        [HttpPatch("Reading/{id}")]
        public async Task<ActionResult<ReadingSectionResponseModel>> UpdateReadingSection(int id, [FromBody] UpdateRSectionRequestModel request)
        {
            var section = await database.ReadingSections.FindAsync(id);

            if (section == null)
                return NotFound("Can't find reading section with id " + id);
            
            if(request.Title != null) section.Title = request.Title;
            if(request.Content != null) section.Content = request.Content;

            await database.SaveChangesAsync();

            return Ok(ReadingSectionToResponseModel(section));
        }

        /// <summary>
        /// Update the listening section information.
        /// </summary>
        [HttpPatch("Listening/{id}")]
        public async Task<ActionResult<ListeningSectionResponseModel>> UpdateListeningSection(int id, [FromBody] UpdateLSectionRequestModel request)
        {
            var section = await database.ListeningSections.FindAsync(id);

            if (section == null)
                return NotFound("Can't find listening section with id " + id);

            if (request.TimeStamp != null) section.TimeStamp = (TimeOnly)request.TimeStamp;
            if (request.Transcript != null) section.Transcript = request.Transcript;

            return Ok(ListeningSectionToResponseModel(section));
        }

        /// <summary>
        /// Upload an image for the reading section.
        /// </summary>
        [HttpPost("Image/{id}")]
        public async Task<IActionResult> UploadReadingSectionImage(int id, IFormFile file)
        {
            var section = await database.ReadingSections.FindAsync(id);

            if (section == null)
                return NotFound("Can't find section with id " + id);

            if (!ResourcesManager.IsImageValid(file))
                return BadRequest("Invalid image");

            ResourcesManager.RemoveFile(section.ImageLink);

            Directory.CreateDirectory(ResourcesManager.sectionsDir);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"section_{id}{fileExtension}";
            var filePath = Path.Combine(ResourcesManager.avatarsDir, fileName);

            await ResourcesManager.SaveImage(file, filePath);

            var relativePath = ResourcesManager.GetRelativePath(filePath);
            section.ImageLink = relativePath;
            await database.SaveChangesAsync();

            var url = $"{Request.Scheme}://{Request.Host}{relativePath}";
            return Ok(url);
        }
    }
}
