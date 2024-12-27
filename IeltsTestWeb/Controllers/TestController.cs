using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;
using System.Text;
using System.Text.RegularExpressions;

namespace IeltsTestWeb.Controllers
{
    [Route("test")]
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

        /// <summary>
        /// Get all tests in the system.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResponseModel>>> GetAllTests()
        {
            var tests = await database.Tests.ToListAsync();
            var responseList = tests.Select(test => Mapper.TestToResponseModel(test)).ToList();
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

            return Ok(Mapper.TestToResponseModel(test));
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
            return Ok(Mapper.TestToResponseModel(test));
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

            return Ok(Mapper.TestToResponseModel(test));
        }

        /// <summary>
        /// Find all tests that match the query parameters.
        /// </summary>
        [HttpGet("match")]
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

            var responseList = tests.Select(test => Mapper.TestToResponseModel(test));

            return Ok(responseList);
        }

        /// <summary>
        /// Validate the test before saving.
        /// </summary>
        [HttpGet("validate/{id}")]
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

        /// <summary>
        /// Upload full test file.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadReadingTest(IFormFile file)
        {
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName) != ".docx")
                return BadRequest("Invalid file");

            string content;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                try
                {
                    using (var doc = new XWPFDocument(memoryStream))
                    {
                        StringBuilder docContent = new StringBuilder();
                        foreach (var paragraph in doc.Paragraphs)
                        {
                            docContent.Append(paragraph.Text);
                        }

                        content = docContent.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error reading DOCX file: {ex.Message}");
                }
            }

            var jsonObjects = ParseJsonFromDoc(content);

            var test = new Test();
            var sections = new List<ReadingSection>();
            var questionLists = new List<QuestionList>();
            var questions = new List<Question>();

            foreach (var jsonObject in jsonObjects)
            {
                string key = jsonObject.Key;
                string json = jsonObject.Value;

                if (key.StartsWith("test"))
                {
                    var readTest = JsonConvert.DeserializeObject<Test>(json);
                    if (readTest != null)
                    {
                        database.Tests.Add(readTest);
                        await database.SaveChangesAsync();

                        test = readTest;
                    }
                }
                else if (key.StartsWith("section"))
                {
                    var section = JsonConvert.DeserializeObject<ReadingSection>(json);
                    if(section != null)
                    {
                        section.TestId = test.TestId;
                        database.ReadingSections.Add(section);
                        await database.SaveChangesAsync();

                        sections.Add(section);
                    }
                }
                else if (key.StartsWith("questionList"))
                {
                    var questionList = JsonConvert.DeserializeObject<QuestionList>(json);
                    if (questionList != null)
                    {
                        questionList.Rsections.Add(sections.Last());
                        database.QuestionLists.Add(questionList);
                        await database.SaveChangesAsync();

                        questionLists.Add(questionList);
                    }
                }
                else if (key.StartsWith("matchingQuestionList"))
                {
                    var matchingQuestionList = JsonConvert.DeserializeObject<MatchQuestionList>(json);
                    if (matchingQuestionList != null)
                    {
                        matchingQuestionList.QlistId = questionLists.Last().QlistId;
                        database.MatchQuestionLists.Add(matchingQuestionList);
                        await database.SaveChangesAsync();
                    }
                }
                else if (key.StartsWith("question"))
                {
                    var question = JsonConvert.DeserializeObject<Question>(json);
                    if(question != null)
                    {
                        question.QlistId = questionLists.Last().QlistId;
                        database.Questions.Add(question);
                        await database.SaveChangesAsync();

                        questions.Add(question);
                    }
                }
                else if (key.StartsWith("explanation"))
                {
                    var explanation = JsonConvert.DeserializeObject<Explanation>(json);
                    if (explanation != null)
                    {
                        explanation.QuestionId = questions.Last().QuestionId;
                        database.Explanations.Add(explanation);
                        await database.SaveChangesAsync();
                    }
                }
            }

            return Ok(jsonObjects);
        }

        private Dictionary<string, string> ParseJsonFromDoc(string content)
        {
            var result = new Dictionary<string, string>();
            var matches = Regex.Matches(content, @"“(\w+)”:\s*({.*?})", RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                string json = match.Groups[2].Value.Replace("“", "\"").Replace("”", "\"");
                result[key] = json;
            }

            return result;
        }
    }
}
