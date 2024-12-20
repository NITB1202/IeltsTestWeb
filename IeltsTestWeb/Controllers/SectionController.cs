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

        /// <summary>
        /// Create a new reading section for the reading test.
        /// </summary>
        [HttpPost("Reading")]
        public async Task<ActionResult<ReadingSectionResponseModel>> CreateReadingSection([FromBody] ReadingSectionRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var test = await database.Tests.FindAsync(request.TestId);

            if (test == null)
                return NotFound("Can't find test with id " + request.TestId);

            if (test.TestSkill != "reading")
                return BadRequest("Can't add a reading section to a listening test");

            var maxSectionNum = await database.Constants.FindAsync("readingSectionNum");
            if (maxSectionNum == null)
                return NotFound("Can't find reading section limit");

            var sectionCount = await database.ReadingSections.CountAsync(section => section.TestId == request.TestId);
            if (sectionCount == maxSectionNum.Value)
                return BadRequest("The test has reached its maximum section.");

            var section = new ReadingSection
            {
                Title = request.Title,
                Content = request.Content,
                TestId = request.TestId
            };

            database.ReadingSections.Add(section);
            await database.SaveChangesAsync();

            return Ok(Mapper.ReadingSectionToResponseModel(section));
        }

        /// <summary>
        /// Create a new listening section for the sound.
        /// </summary>
        [HttpPost("Listening")]
        public async Task<ActionResult<ListeningSectionResponseModel>> CreateListeningSection([FromBody] ListeningSectionRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await database.Sounds.AnyAsync(sound => sound.SoundId == request.SoundId))
                return NotFound("Can't find sound with id " + request.SoundId);

            var sections = database.ListeningSections.Where(s => s.SoundId == request.SoundId);
            var sectionCount = sections.Count();

            var maxSectionNum = await database.Constants.FindAsync("listeningSectionNum");
            if (maxSectionNum == null)
                return NotFound("Can't find listening section limit");
            if(sectionCount == maxSectionNum.Value)
                return BadRequest("The test has reached its maximum section.");

            // Check dupplicate
            if (sections.Any(s => s.SectionOrder == request.SectionOrder))
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

            return Ok(Mapper.ListeningSectionToResponseModel(section));
        }

        /// <summary>
        /// Get all sections belong to the test.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> FindAllTestSections(int id)
        {
            var test = await database.Tests.FindAsync(id);

            if (test == null)
                return NotFound("Can't find test with id " + id);

            if(test.TestSkill == "reading")
            {
                var sections = database.ReadingSections.Where(s => s.TestId == id);
                var responseList = await sections.Select(s => Mapper.ReadingSectionToResponseModel(s)).ToListAsync();
                var questionLists = await database.QuestionLists.Include(ql => ql.Rsections).ToListAsync();

                foreach(ReadingSectionResponseModel section in responseList) 
                {
                    int questionNum = 0;
                    
                    foreach(var questionList in questionLists) 
                    {
                        var questionListSection = questionList.Rsections.FirstOrDefault();
                        
                        if (questionListSection != null && questionListSection.RsectionId == section.Id)
                            questionNum += questionList.Qnum;
                    }
                  
                    section.QuestionNum = questionNum;
                }

                return Ok(responseList);
            }

            if(test.TestSkill == "listening")
            {
                var soundId = await database.Sounds
                    .Where(sound => sound.TestId == id)
                    .Select(sound => sound.SoundId)
                    .FirstOrDefaultAsync();

                var sections = database.ListeningSections.Where(s => s.SoundId == soundId);
                var responseList = await sections.Select(s => Mapper.ListeningSectionToResponseModel(s)).ToListAsync();
                var questionLists = await database.QuestionLists.Include(ql => ql.Lsections).ToListAsync();

                foreach(ListeningSectionResponseModel section in responseList) 
                {
                    int questionNum = 0;

                    foreach(var questionList in questionLists)
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

            return Ok(Mapper.ReadingSectionToResponseModel(section));
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

            return Ok(Mapper.ListeningSectionToResponseModel(section));
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
            var filePath = Path.Combine(ResourcesManager.sectionsDir, fileName);

            await ResourcesManager.SaveImage(file, filePath, 1200, 1200);

            var relativePath = ResourcesManager.GetRelativePath(filePath);
            section.ImageLink = relativePath;
            await database.SaveChangesAsync();

            var url = $"{Request.Scheme}://{Request.Host}{relativePath}";
            return Ok(url);
        }

        /// <summary>
        /// Get all the test section details.
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<ReadingSectionDetailsResponseModel>> GetReadingTestDetails(int id)
        {
            var readingSections = await database.ReadingSections.Where(section => section.TestId == id).ToListAsync();
            var responses = new List<ReadingSectionDetailsResponseModel>();

            if (readingSections == null)
                return NotFound("No section found with test id " + id);

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
                    foreach ( var question in questions)
                    {
                        QuestionDetailsResponseModel questionResponse = new QuestionDetailsResponseModel();
                        questionResponse.Question = Mapper.QuestionToResponseModel(question);

                        var explanation = await database.Explanations.FirstOrDefaultAsync(e => e.QuestionId == question.QuestionId);
                        questionResponse.Explanation = explanation != null? Mapper.ExplanationToResponseModel(explanation) : new ExplanationResponseModel();

                        qlResponse.questions.Add(questionResponse);
                    }


                    response.QuestionLists.Add(qlResponse);
                }

                responses.Add(response);
            }

            return Ok(responses);
        }
    }
}
