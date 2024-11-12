using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Principal;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TestController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public TestController(ieltsDbContext database)
        {
            this.database = database;
        }
        private async Task<bool> IsDupplicated(TestRequestModel model)
        {
            if (await database.Tests.AnyAsync(test =>
                test.TestType == model.TestType &&
                test.TestSkill == model.TestSkill &&
                test.Name == model.Name &&
                test.MonthEdition == model.MonthEdition &&
                test.YearEdition == model.YearEdition
            ))
                return true;

            return false;
        }
        private static TestResponseModel TestToResponseModel(Test model)
        {
            return new TestResponseModel
            {
                TestId = model.TestId,
                TestType = model.TestType,
                TestSkill = model.TestSkill,
                Name = model.Name,
                MonthEdition = model.MonthEdition,
                YearEdition = model.YearEdition,
                UserCompletedNum = model.UserCompletedNum
            };
        }

        /// <summary>
        /// Get all tests in the system.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResponseModel>>> GetAllTests()
        {
            var tests = await database.Tests.ToListAsync();
            var responseList = tests.Select(test => TestToResponseModel(test)).ToList();
            return Ok(responseList);
        }
        
        /// <summary>
        /// Get test by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestResponseModel>> FindTestById(int id)
        {
            var test = await database.Tests.FindAsync(id);

            if (test == null)
                return NotFound("Can't find test with id " + id);

            return Ok(TestToResponseModel(test));
        }
        
        /// <summary>
        /// Create new test.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TestResponseModel>> CreateNewTest([FromBody] TestRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await IsDupplicated(request))
                return BadRequest("The test already exists");

            var test = new Test
            {
                TestType = request.TestType,
                TestSkill = request.TestSkill,
                Name = request.Name,
                MonthEdition = request.MonthEdition,
                YearEdition = request.YearEdition
            };

            database.Tests.Add(test);
            await database.SaveChangesAsync();
            return Ok(TestToResponseModel(test));
        }

        /// <summary>
        /// Update the test information.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<TestResponseModel>> UpdateTest(int id, [FromBody] TestRequestModel request)
        {
            var test = await database.Tests.FindAsync(id);

            if (test == null)
                return NotFound("Can't find test with id " + id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await IsDupplicated(request))
                return BadRequest("The specific test already exists. Please change the updated value.");

            var requestProperties = typeof(TestRequestModel).GetProperties();
            var testProperties = typeof(Test).GetProperties();

            foreach (var prop in requestProperties)
            {
                var requestValue = prop.GetValue(request);

                // If the value is not null, find corresponding property in test sample and update
                if (requestValue != null)
                {
                    var testProp = testProperties.FirstOrDefault(p => p.Name == prop.Name);
                    if (testProp != null && testProp.CanWrite)
                        testProp.SetValue(test, requestValue);
                }
            }

            await database.SaveChangesAsync();

            return Ok(TestToResponseModel(test));
        }

        /// <summary>
        /// Find all tests that match the query parameters.
        /// </summary>
        [HttpGet("Match")]
        public ActionResult<IEnumerable<TestResponseModel>> FindTestsMatch(
            [FromQuery] string? name, [FromQuery] string? testType, [FromQuery] string? testSkill,
            [FromQuery] int? monthEdition, [FromQuery] int? yearEdition)
        {
            var tests = database.Tests.Where(test =>
                (name == null || test.Name.ToLower().StartsWith(name.ToLower())) &&
                (testType == null || testType == test.TestType) &&
                (testSkill == null || testSkill == test.TestSkill) &&
                (monthEdition == null || monthEdition == test.MonthEdition) &&
                (yearEdition == null || yearEdition == test.YearEdition)
            );

            var responseList = tests.Select(test => TestToResponseModel(test));

            return Ok(responseList);
        }

        /// <summary>
        /// Validate the test before saving.
        /// </summary>
        [HttpGet("Validate/{id}")]
        public async Task<ActionResult> ValidateTest(int id)
        {
            // Find test
            var test = await database.Tests.FindAsync(id);
            if (test == null)
                return NotFound("Can't find test with id " + id);

            var qlists = new List<QuestionList>();
            var questionNum = 0;

            if (test.TestSkill == "reading")
            {
                foreach(var qlist in database.QuestionLists.Include(q => q.Rsections))
                {
                    var section = qlist.Rsections.FirstOrDefault();
                    if (section != null && section.TestId == id)
                        qlists.Add(qlist);
                }
            }    

            if(test.TestSkill == "listening")
            {
                var sound = await database.Sounds.FirstOrDefaultAsync(s => s.TestId == id);
                if (sound == null)
                    return NotFound("Can't find sound for this test");

                foreach(var qlist in database.QuestionLists.Include(q => q.Lsections))
                {
                    var section = qlist.Lsections.FirstOrDefault();
                    if (section != null && section.SoundId == sound.SoundId)
                        qlists.Add(qlist);
                }
            }

            foreach (var qlist in qlists)
                questionNum += qlist.Qnum;

            if (questionNum == 40)
                return Ok("The test is valid");

            return BadRequest("The test should consist of 40 questions. Curent questions: " + questionNum);
        }
    }
}
