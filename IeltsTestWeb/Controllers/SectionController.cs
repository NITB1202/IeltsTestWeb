using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public SectionController(ieltsDbContext database)
        {
            this.database = database;
        }

        [HttpPost("Reading/{testId}")]
        public async Task<IActionResult> CreateReadingSectionForTest(int testId, [FromBody] ReadingSection request)
        {
            var test = await database.Tests.FindAsync(testId);

            if (test == null)
                return NotFound("Can't find test with id " + testId);

            if (test.TestSkill != "reading")
                return BadRequest("Can't add a reading section to a listening test");


            return Ok();
        }

        // Add a listening section

        // Find all section belongs to test with id

        //

    }
}
