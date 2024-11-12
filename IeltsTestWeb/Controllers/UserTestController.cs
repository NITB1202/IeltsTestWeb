using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserTestController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public UserTestController(ieltsDbContext database)
        {
            this.database = database;
        }
        private static UserTestResponseModel UserTestToResponseModel(UserTest model)
        {
            return new UserTestResponseModel
            {
                Id = model.UtestId,
                AccountId = model.AccountId,
                Name = model.Name,
                DateCreate = model.DateCreate,
                TestType = model.TestType,
                TestSkill = model.TestSkill
            };
        }
        private static DetailResponseModel DetailToResponseModel(UserTestDetail model)
        {
            return new DetailResponseModel
            {
                Id = model.TdetailId,
                TestId = model.UtestId,
                SectionId = model.SectionId
            };
        }

        /// <summary>
        /// Create a new user test.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserTestResponseModel>> CreateNewUserTest([FromBody] UserTestRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await database.Accounts.AnyAsync(account => account.AccountId == request.AccountId))
                return BadRequest("Can't find account with id " + request.AccountId);

            if (await database.UserTests.AnyAsync(test => test.AccountId == request.AccountId && test.Name == request.Name))
                return BadRequest("The test with this name already exists");

            var test = new UserTest
            {
                AccountId = request.AccountId,
                Name = request.Name,
                TestType = request.TestType,
                TestSkill = request.TestSkill
            };

            database.UserTests.Add(test);
            await database.SaveChangesAsync();

            return Ok(UserTestToResponseModel(test));
        }

        /// <summary>
        /// Create a reading test details featuring matching types.
        /// </summary>
        [HttpPost("Reading")]
        public async Task<IActionResult> CreateReadingTestDetail(DetailRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var test = await database.UserTests.FindAsync(request.TestId);

            if (test == null)
                return NotFound("Can't find user's test with id " + request.TestId);

            if (test.TestSkill != "reading")
                return BadRequest("Test skill is not reading");

            var sectionIds = new List<int>();

            foreach (var type in request.Types)
            {
                foreach (var qlist in database.QuestionLists.Include(qlist => qlist.Rsections))
                {
                    if (qlist.QlistType == type)
                    {
                        var section = qlist.Rsections.FirstOrDefault();
                        if (section != null && !sectionIds.Contains(section.RsectionId))
                        {
                            sectionIds.Add(section.RsectionId);
                            break;
                        }
                    }
                }
            }

            if (sectionIds.Count != request.Types.Count)
                return BadRequest("Can't find a test that matches all the types described.");

            foreach (var sectionId in sectionIds)
            {
                var detail = new UserTestDetail
                {
                    UtestId = request.TestId,
                    SectionId = sectionId
                };

                database.UserTestDetails.Add(detail);
            }

            await database.SaveChangesAsync();

            return Ok(new { sectionIds = sectionIds });
        }

        /// <summary>
        /// Create a reading test details featuring matching types.
        /// </summary>
        /// <returns></returns>
        [HttpPost("Listening")]
        public async Task<IActionResult> CreateListeningTestDetail(DetailRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var test = await database.UserTests.FindAsync(request.TestId);

            if (test == null)
                return NotFound("Can't find user's test with id " + request.TestId);

            if (test.TestSkill != "listening")
                return BadRequest("Test skill is not listening");

            foreach(var sound in database.Sounds)
            {

            }
            return Ok();
        }

        /// <summary>
        /// Get all the tests that the user has created.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllUserTest(int id)
        {
            var tests = database.UserTests.Where(test => test.AccountId == id);
            var responseList = await database.UserTests.Select(test => UserTestToResponseModel(test)).ToListAsync();
            return Ok(responseList);
        }

        /// <summary>
        /// Get the details of the test.
        /// </summary>
        [HttpGet("Detail/{id}")]
        public async Task<ActionResult<IEnumerable<DetailResponseModel>>> GetAllTestDetails(int id)
        {
            var details = database.UserTestDetails.Where(detail => detail.UtestId == id);
            var responseList = await details.Select(detail => DetailToResponseModel(detail)).ToListAsync();
            return Ok(responseList);
        }

        /// <summary>
        /// Update the name of the test.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<UserTestResponseModel>> UpdateUserTest(int id, [FromQuery] string name)
        {
            var test = await database.UserTests.FindAsync(id);

            if (test == null)
                return NotFound("Can't find user's test with id " + id);

            if (!await database.UserTests.AnyAsync(utest => utest.AccountId == test.AccountId && utest.Name == test.Name))
                return BadRequest("This name is already in use.");

            test.Name = name;

            await database.SaveChangesAsync();

            return (UserTestToResponseModel(test));
        }

        /// <summary>
        /// Find all user tests that match the query parameters.
        /// </summary>
        [HttpGet("Match")]
        public ActionResult<IEnumerable<UserTestResponseModel>> FindAllUserTestsMatch(
            [FromQuery] int? accountId, [FromQuery] string? name, [FromQuery] string? testType, [FromQuery] string? testSkill)
        {
            if (accountId == null)
                return BadRequest("Account id can't be null");

            var tests = database.UserTests.Where(test =>
                (accountId == test.AccountId) &&
                (name == null || test.Name.ToLower().StartsWith(name.ToLower())) &&
                (testType == null || testType == test.TestType) &&
                (testSkill == null || testSkill == test.TestSkill)
            );

            var responseList = tests.Select(test => UserTestToResponseModel(test));

            return Ok(responseList);
        }
    }
}