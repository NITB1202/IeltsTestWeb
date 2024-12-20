using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Dml;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ResultController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public ResultController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Create new test result.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResultResponseModel>> CreateNewResult([FromBody] ResultRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await database.Accounts.AnyAsync(ac => ac.AccountId == request.AccountId))
                return NotFound("Can't find acccount with id " + request.AccountId);

            var testSkill = "";
            var publicTest = new Test();

            if(request.TestAccess == "public")
            {
                var test = await database.Tests.FindAsync(request.TestId);

                if (test == null)
                    return NotFound("Can't find test with id " + request.TestId);

                testSkill = test.TestSkill;
                publicTest = test;
            }

            if(request.TestAccess == "private")
            {
                var test = await database.UserTests.FirstOrDefaultAsync(utest =>
                utest.AccountId == request.AccountId && utest.UtestId == request.TestId);

                if (test == null)
                    return NotFound("Can't find user's test with id " + request.TestId);

                testSkill = test.TestSkill;
            }

            var maxMinutes = 0;

            if(testSkill == "reading")
            { 
                var readingTime = await database.Constants.FindAsync("readingTime");

                if(readingTime == null)
                    return NotFound("Can't find the maximum time value for the reading test");

                maxMinutes = (int)readingTime.Value;
            }

            if(testSkill == "listening")
            {
                var listeningTime = await database.Constants.FindAsync("listeningTime");
                
                if(listeningTime == null)
                    return NotFound("Can't find the maximum time value for the listening test");

                maxMinutes = (int)listeningTime.Value;
            }

            var MaxTime = new TimeOnly(maxMinutes / 60, maxMinutes % 60);

            if(request.CompleteTime > MaxTime)
                return BadRequest("The time completed must be less than or equal to " + maxMinutes + " minutes");

            if(request.TestAccess == "public")
                publicTest.UserCompletedNum += 1;

            var result = new Result
            {
                AccountId = request.AccountId,
                TestId = request.TestId,
                TestAccess = request.TestAccess,
                CompleteTime = request.CompleteTime
            };

            database.Results.Add(result);

            await database.SaveChangesAsync();

            return Ok(Mapper.ResultToResponseModel(result));
        }

        /// <summary>
        /// Create result details.
        /// </summary>
        [HttpPost("Detail")]
        public async Task<ActionResult<ResultDetailResponseModel>> CreateNewDetail([FromBody] ResultDetailRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await database.Results.FindAsync(request.ResultId);
            
            if (result == null)
                return NotFound("Can't find result with id" + request.ResultId);

            if (await database.ResultDetails.AnyAsync(detail => 
                detail.ResultId == request.ResultId && detail.QuestionOrder == request.QuestionOrder))
                return BadRequest("The result details for this question already exist");

            var ex = await database.Explanations.FirstOrDefaultAsync(e => e.QuestionId == request.QuestionId);
            if (ex == null)
                return NotFound("Cant't find an explanation for the question with id " + request.QuestionId);

            var detail = new ResultDetail
            {
                ResultId = request.ResultId,
                QuestionOrder = request.QuestionOrder,
                QuestionId = request.QuestionId,
                UserAnswer = request.UserAnswer
            };

            if (ex.Content.Equals(request.UserAnswer.TrimEnd()))
            {
                detail.QuestionState = "right";
                result.Score += 1;
            }
            else
                detail.QuestionState = "wrong";

            database.ResultDetails.Add(detail);
            await database.SaveChangesAsync();

            return Ok(Mapper.ResultDetailToResponseModel(detail));
        }

        /// <summary>
        /// Get all results belong to the account.
        /// </summary>
        [HttpGet("Account/{id}")]
        public async Task<ActionResult<IEnumerable<ResultResponseModel>>> GetAllResults(int id)
        {
            var results = database.Results.Where(result => result.AccountId == id);
            var responseList = await results.Select(result => Mapper.ResultToResponseModel(result)).ToListAsync();
            return Ok(responseList);
        }

        /// <summary>
        /// Get result by id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultResponseModel>> GetResult(int id)
        {
            var result = await database.Results.FindAsync(id);

            if (result == null)
                return NotFound("Can't find result with id " + id);

            return Ok(Mapper.ResultToResponseModel(result));
        }

        /// <summary>
        /// Get all result details by result id.
        /// </summary>
        [HttpGet("Detail/{id}")]
        public async Task<ActionResult<IEnumerable<ResultDetailResponseModel>>> GetAllDetails(int id)
        {
            var details = database.ResultDetails.Where(detail => detail.ResultId == id);
            var responseList = await details
                .Select(detail => Mapper.ResultDetailToResponseModel(detail))
                .ToListAsync();

            responseList = responseList.OrderBy(detail => detail.QuestionOrder).ToList();
            return Ok(responseList);
        }

        /// <summary>
        /// Find all results that match the query parameters.
        /// </summary>
        [HttpGet("Match")]
        public async Task<ActionResult<IEnumerable<ResultResponseModel>>> GetResultsMatch(
            [FromQuery] string? testName, [FromQuery] string? testAccess, [FromQuery] string? testType,
            [FromQuery] string? testSkill, [FromQuery] int? accountId)
        {
            if (accountId == null)
                return BadRequest("Account id can't be null");

            if (!await database.Accounts.AnyAsync(account => account.AccountId == accountId))
                return NotFound("Can't find account with id " + accountId);

            var results = new List<Result>();

            if (testAccess == null || testAccess == "public")
            {
                var testIds = await database.Tests.Where(test =>
                    (testName == null || test.Name.ToLower().StartsWith(testName.ToLower())) &&
                    (testType == null || testType == test.TestType) &&
                    (testSkill == null || testSkill == test.TestSkill)
                ).Select(test => test.TestId).ToListAsync();

                foreach (var testId in testIds)
                {
                    foreach (var result in database.Results)
                        if (result.TestId == testId && result.TestAccess == "public" && result.AccountId == accountId)
                        {
                            results.Add(result);
                            break;
                        }
                }
            }

            if (testAccess == null || testAccess == "private")
            {
                var utestIds = await database.UserTests.Where(test =>
                    (testName == null || test.Name.ToLower().StartsWith(testName.ToLower())) &&
                    (testType == null || testType == test.TestType) &&
                    (testSkill == null || testSkill == test.TestSkill) &&
                    (accountId == test.AccountId)
                ).Select(test => test.UtestId).ToListAsync();

                foreach (var utestId in utestIds)
                {
                    foreach (var result in database.Results)
                        if (result.TestId == utestId && result.TestAccess == "private" && result.AccountId == accountId)
                        {
                            results.Add(result);
                            break;
                        }
                }
            }

            var responseList = results.Select(result => Mapper.ResultToResponseModel(result));

            return Ok(responseList);
        }

        /// <summary>
        /// Get user's score.
        /// </summary>
        [HttpPost("Full")]
        public async Task<ActionResult<int>> CreateListResultDetails([FromBody] ListResultDetailsRequestModel request)
        {
            var result = await database.Results.FindAsync(request.resultId);
            if (result == null)
                return NotFound("Can't find result with id " + request.resultId);

            for(int i = 0; i< request.questionIds.Count; i++)
            {
                var questionId = request.questionIds[i];
                var userAnswer = request.userAnswers[i + 1];

                var question = await database.Questions.FindAsync(questionId);
                if (question == null)
                    return NotFound("Can't find question with id" + questionId);

                string state = question.Answer.ToLower().Trim().Equals(userAnswer.ToLower().Trim()) ? "right" : "wrong";

                if(state == "right")
                {
                    result.Score += 1;
                }

                var detail = new ResultDetail
                {
                    ResultId = request.resultId,
                    QuestionOrder = i + 1,
                    QuestionId = questionId,
                    UserAnswer = userAnswer,
                    QuestionState = state,
                };

                database.ResultDetails.Add(detail);
            }

            await database.SaveChangesAsync();

            return Ok(result.Score);
        }
    }
}
