using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IeltsTestWeb.Controllers
{
    [Route("usertest")]
    [ApiController]
    [Produces("application/json")]
    public class UserTestController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public UserTestController(ieltsDbContext database)
        {
            this.database = database;
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

            return Ok(Mapper.UserTestToResponseModel(test));
        }

        /// <summary>
        /// Create a reading test details featuring matching types.
        /// </summary>
        [HttpPost("reading")]
        public async Task<IActionResult> CreateReadingTestDetail(DetailRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var test = await database.UserTests.FindAsync(request.TestId);

            if (test == null)
                return NotFound("Can't find user's test with id " + request.TestId);

            if (test.TestSkill != "reading")
                return BadRequest("Test skill is not reading");

            var candidates = new Dictionary<string, List<int>>();

            foreach (var type in request.Types)
            {
                if (!candidates.ContainsKey(type))
                    candidates[type] = new List<int>();

                // Get question list along with reading section.
                foreach (var qlist in database.QuestionLists.Include(qlist => qlist.Rsections))
                {
                    // If the question list type fits, check if it is from the reading section.

                    if (qlist.QlistType == type)
                    {
                        var section = qlist.Rsections.FirstOrDefault();

                        // If it from a reading section, add this section to list of candidates
                        if (section != null)
                            candidates[type].Add(section.RsectionId);
                    }
                }
            }

            var allResults = new List<List<int>>();
            var usedItemIds = new HashSet<int>();

            FindAllSelectionsRecursively(request.Types, candidates, 0, usedItemIds, new List<int>(), allResults);

            if (allResults.Count == 0)
                return BadRequest("Can't find a test that matches all the types described.");

            var index = new Random().Next(0, allResults.Count);
            var sectionIds = allResults[index];

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
        private static void FindAllSelectionsRecursively(List<string> conditions, Dictionary<string, List<int>> candidates,
                                                  int index, HashSet<int> usedItemIds, List<int> currentSelection, List<List<int>> allResults)
        {
            if (index == conditions.Count)
            {
                // When all conditions are met, include this selection in the result.
                allResults.Add(new List<int>(currentSelection));
                return;
            }

            var condition = conditions[index];
            foreach (var item in candidates[condition])
            {
                if (!usedItemIds.Contains(item))
                {
                    // Select the item for current condition
                    usedItemIds.Add(item);
                    currentSelection.Add(item);

                    // Recursive to find all possible sets for the next condition
                    FindAllSelectionsRecursively(conditions, candidates, index + 1, usedItemIds, currentSelection, allResults);

                    // Remove this item to continue with the other selection
                    usedItemIds.Remove(item);
                    currentSelection.RemoveAt(currentSelection.Count - 1);
                }
            }
        }

        /// <summary>
        /// Create a listening test details featuring matching types.
        /// </summary>
        /// <returns></returns>
        [HttpPost("listening")]
        public async Task<IActionResult> CreateListeningTestDetail(DetailRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var test = await database.UserTests.FindAsync(request.TestId);

            if (test == null)
                return NotFound("Can't find user's test with id " + request.TestId);

            if (test.TestSkill != "listening")
                return BadRequest("Test skill is not listening");

            // Create new dictionary to store soundId and available question list type
            var candidates = new Dictionary<int, List<string>>();

            foreach(var qlist in database.QuestionLists.Include(q => q.Lsections))
            {
                if(request.Types.Contains(qlist.QlistType))
                {
                    var section = qlist.Lsections.FirstOrDefault();
                    if(section != null)
                    {
                        var soundId = section.SoundId;

                        if (!candidates.ContainsKey(soundId))
                            candidates[soundId] = new List<string>();

                        candidates[soundId].Add(qlist.QlistType);
                    }

                }
            }

            var typeCounts = request.Types
                .GroupBy(condition => condition)
                .ToDictionary(group => group.Key, group => group.Count());

            var possibleResults = new List<int>();

            foreach (var candidate in candidates)
            {
                // Count the number of each type occur in the sound available type list
                var itemConditionCounts = candidate.Value
                    .GroupBy(condition => condition)
                    .ToDictionary(group => group.Key, group => group.Count());

                // Check if each type occur in the sound available type list has enough occurence in type list
                bool satisfiesAll = true;
                foreach (var type in typeCounts)
                {
                    if (!itemConditionCounts.TryGetValue(type.Key, out int count) || count < type.Value)
                    {
                        satisfiesAll = false;
                        break;
                    }
                }

                if (satisfiesAll)
                {
                    possibleResults.Add(candidate.Key);
                }
            }

            if (possibleResults.Count == 0)
                return BadRequest("Can't find a test that matches all the types described.");

            var index = new Random().Next(0, possibleResults.Count);

            var sectionIds = database.ListeningSections
                                    .Where(section => section.SoundId == possibleResults[index])
                                    .Select(section => section.LsectionId);

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
        /// Get all the tests that the user has created.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllUserTest(int id)
        {
            var tests = database.UserTests.Where(test => test.AccountId == id);
            var responseList = await database.UserTests.Select(test => Mapper.UserTestToResponseModel(test)).ToListAsync();
            return Ok(responseList);
        }

        /// <summary>
        /// Get the details of the test.
        /// </summary>
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<IEnumerable<DetailResponseModel>>> GetAllTestDetails(int id)
        {
            var details = database.UserTestDetails.Where(detail => detail.UtestId == id);
            var responseList = await details.Select(detail => Mapper.DetailToResponseModel(detail)).ToListAsync();
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

            return (Mapper.UserTestToResponseModel(test));
        }

        /// <summary>
        /// Find all user tests that match the query parameters.
        /// </summary>
        [HttpGet("match")]
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

            var responseList = tests.Select(test => Mapper.UserTestToResponseModel(test));

            return Ok(responseList);
        }

        /// <summary>
        /// Get user test by id.
        /// </summary>
        [HttpGet("info/{id}")]
        public async Task<ActionResult<UserTestResponseModel>> GetUserTestById(int id)
        {
            var test = await database.UserTests.FindAsync(id);
            var response = test != null? Mapper.UserTestToResponseModel(test) : null;
            return Ok(response);
        }

        /// <summary>
        /// Get full reading test.
        /// </summary>
        [HttpGet("reading/{id}")]
        public async Task<ActionResult<ReadingSectionDetailsResponseModel>> GetReadingTestDetails(int id)
        {
            var readingSectionIds = await database.UserTestDetails
                .Where(test => test.UtestId == id)
                .Select(test => test.SectionId)
                .ToListAsync();

            var readingSections = new List<ReadingSection>();

            foreach(var readingSectionId in readingSectionIds)
            {
                var readingSection = await database.ReadingSections.FindAsync(readingSectionId);
                if (readingSection != null) readingSections.Add(readingSection);
            }

            if (readingSections.Count == 0)
                return NotFound("No section found with test id " + id);

            var responses = new List<ReadingSectionDetailsResponseModel>();

            foreach (var section in readingSections)
            {
                ReadingSectionDetailsResponseModel response = new ReadingSectionDetailsResponseModel();
                response.Section = Mapper.ReadingSectionToResponseModel(section);
                response.QuestionLists = new List<QuestionListDetailResponseModel>();
                response.Section.QuestionNum = 0;

                var questionLists = await database.QuestionLists
                                    .Include(ql => ql.Rsections)
                                    .Where(ql => ql.Rsections.Any(rs => rs.RsectionId == section.RsectionId))
                                    .ToListAsync();

                foreach (var questionList in questionLists)
                {
                    QuestionListDetailResponseModel qlResponse = new QuestionListDetailResponseModel();
                    qlResponse.questionList = Mapper.QuestionListToResponseModel(questionList);
                    qlResponse.questions = new List<QuestionDetailsResponseModel>();
                    response.Section.QuestionNum += questionList.Qnum;

                    var questions = await database.Questions.Where(q => q.QlistId == questionList.QlistId).ToListAsync();
                    foreach (var question in questions)
                    {
                        QuestionDetailsResponseModel questionResponse = new QuestionDetailsResponseModel();
                        questionResponse.Question = Mapper.QuestionToResponseModel(question);

                        var explanation = await database.Explanations.FirstOrDefaultAsync(e => e.QuestionId == question.QuestionId);
                        questionResponse.Explanation = explanation != null ? Mapper.ExplanationToResponseModel(explanation) : new ExplanationResponseModel();

                        qlResponse.questions.Add(questionResponse);
                    }


                    response.QuestionLists.Add(qlResponse);
                }

                responses.Add(response);
            }

            return Ok(responses);
        }

        /// <summary>
        /// Get full listening test.
        /// </summary>
        [HttpGet("listening/{id}")]
        public async Task<ActionResult<ReadingSectionDetailsResponseModel>> GetListeningTestDetails(int id)
        {
            var listeningSectionIds = await database.UserTestDetails
                .Where(test => test.UtestId == id)
                .Select(test => test.SectionId)
                .ToListAsync();

            var listeningSections = new List<ListeningSection>();

            foreach (var listeningSectionId in listeningSectionIds)
            {
                var listeningSection = await database.ListeningSections.FindAsync(listeningSectionId);
                if (listeningSection != null) listeningSections.Add(listeningSection);
            }

            if (listeningSections.Count == 0)
                return NotFound("No section found with test id " + id);

            List<ListeningSectionDetailsResponseModel> responses = new List<ListeningSectionDetailsResponseModel>();

            foreach (var section in listeningSections)
            {
                ListeningSectionDetailsResponseModel response = new ListeningSectionDetailsResponseModel();
                response.Section = Mapper.ListeningSectionToResponseModel(section);
                response.QuestionLists = new List<QuestionListDetailResponseModel>();
                response.Section.QuestionNum = 0;

                var questionLists = await database.QuestionLists
                                    .Include(ql => ql.Lsections)
                                    .Where(ql => ql.Lsections.Any(ls => ls.LsectionId == section.LsectionId))
                                    .ToListAsync();

                foreach (var questionList in questionLists)
                {
                    QuestionListDetailResponseModel qlResponse = new QuestionListDetailResponseModel();
                    qlResponse.questionList = Mapper.QuestionListToResponseModel(questionList);
                    qlResponse.questions = new List<QuestionDetailsResponseModel>();
                    response.Section.QuestionNum += questionList.Qnum;

                    var questions = await database.Questions.Where(q => q.QlistId == questionList.QlistId).ToListAsync();
                    foreach (var question in questions)
                    {
                        QuestionDetailsResponseModel questionResponse = new QuestionDetailsResponseModel();
                        questionResponse.Question = Mapper.QuestionToResponseModel(question);

                        var explanation = await database.Explanations.FirstOrDefaultAsync(e => e.QuestionId == question.QuestionId);
                        questionResponse.Explanation = explanation != null ? Mapper.ExplanationToResponseModel(explanation) : new ExplanationResponseModel();

                        qlResponse.questions.Add(questionResponse);
                    }

                    response.QuestionLists.Add(qlResponse);
                }

                responses.Add(response);
            }

            return Ok(responses);
        }

        /// <summary>
        /// Delete the user test.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTest(int id)
        {
            var test = await database.UserTests.FindAsync(id);
            if (test == null) return NotFound("Can't find test with id " + id);

            var details = await database.UserTestDetails
                .Where(detail => detail.UtestId == id)
                .ToListAsync();

            var results = await database.Results
                .Where(result => result.TestAccess == "private" && result.TestId == id)
                .ToListAsync();

            var resultIds = results.Select(result => result.ResultId).ToList();
            var resultDetails = await database.ResultDetails
                .Where(d => resultIds.Contains(d.ResultId))
                .ToListAsync();

            database.RemoveRange(resultDetails);
            database.RemoveRange(results);
            database.RemoveRange(details);
            database.Remove(test);

            await database.SaveChangesAsync();

            return Ok("Delete successfully");
        }

        /// <summary>
        /// Get all sections of the user test.
        /// </summary>
        [HttpGet("all/{id}")]
        public async Task<IActionResult> FindAllTestSections(int id)
        {
            var test = await database.UserTests.FindAsync(id);

            if (test == null)
                return NotFound("Can't find test with id " + id);

            var sectionIds = await database.UserTestDetails
                .Where(detail => detail.UtestId == id)
                .Select(detail => detail.SectionId)
                .ToListAsync();

            if (test.TestSkill == "reading")
            {
                var sections = new List<ReadingSection>();
                foreach(var sectionId in sectionIds)
                {
                    var section = await database.ReadingSections.FindAsync(sectionId);
                    if (section != null)
                        sections.Add(section);
                }

                var responseList = sections.Select(s => Mapper.ReadingSectionToResponseModel(s)).ToList();
                var questionLists = await database.QuestionLists.Include(ql => ql.Rsections).ToListAsync();

                foreach (ReadingSectionResponseModel section in responseList)
                {
                    int questionNum = 0;

                    foreach (var questionList in questionLists)
                    {
                        var questionListSection = questionList.Rsections.FirstOrDefault();

                        if (questionListSection != null && questionListSection.RsectionId == section.Id)
                            questionNum += questionList.Qnum;
                    }

                    section.QuestionNum = questionNum;
                }

                return Ok(responseList);
            }

            if (test.TestSkill == "listening")
            {
                var sections = new List<ListeningSection>();
                foreach (var sectionId in sectionIds)
                {
                    var section = await database.ListeningSections.FindAsync(sectionId);
                    if (section != null)
                        sections.Add(section);
                }

                var responseList = sections.Select(s => Mapper.ListeningSectionToResponseModel(s));
                var questionLists = await database.QuestionLists.Include(ql => ql.Lsections).ToListAsync();

                foreach (ListeningSectionResponseModel section in responseList)
                {
                    int questionNum = 0;

                    foreach (var questionList in questionLists)
                    {
                        var questionListSection = questionList.Lsections.FirstOrDefault();

                        if (questionListSection != null && questionListSection.LsectionId == section.Id)
                            questionNum += questionList.Qnum;
                    }

                    section.QuestionNum = questionNum;
                }

                return Ok(responseList);
            }

            return NoContent();
        }
    }
}