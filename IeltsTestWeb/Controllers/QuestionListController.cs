using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using Microsoft.AspNetCore.Mvc;
using IeltsTestWeb.Utils;
using Microsoft.EntityFrameworkCore;
using IeltsTestWeb.ResponseModels;

namespace IeltsTestWeb.Controllers
{
    [Route("questionlist")]
    [ApiController]
    [Produces("application/json")]
    public class QuestionListController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public QuestionListController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Create new question list.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<QuestionListResponseModel>> CreateNewQuestionList([FromBody] QuestionListRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var qlist = new QuestionList
            {
                QlistType = request.QuestionListType,
                Content = request.Content
            };

            if (request.SectionType == "reading")
            {
                var section = await database.ReadingSections.FindAsync(request.SectionId);
                if (section == null)
                    return NotFound("Can't find reading section with id " + request.SectionId);
                qlist.Rsections.Add(section);
            }

            if (request.SectionType == "listening")
            {
                var section = await database.ListeningSections.FindAsync(request.SectionId);
                if (section == null)
                    return NotFound("Can't find listening section with id " + request.SectionId);
                qlist.Lsections.Add(section);
            }

            database.QuestionLists.Add(qlist);

            await database.SaveChangesAsync();

            return Ok(Mapper.QuestionListToResponseModel(qlist));
        }

        /// <summary>
        /// Upload an image file for the diagram question list.
        /// </summary>
        [HttpPost("image/{id}")]
        public async Task<IActionResult> UploadDiagramImage(int id, IFormFile file)
        {
            var qlist = await database.QuestionLists.FindAsync(id);

            if (qlist == null)
                return NotFound("Can't find question list with id " + id);

            if (qlist.QlistType != "diagram")
                return BadRequest("Can't add an image to a question list that is not the diagram type");

            if (!ResourcesManager.IsImageValid(file))
                return BadRequest("Invalid image file");

            var diagram = await database.DiagramQuestionLists.FirstOrDefaultAsync(d => d.QlistId == id);

            if (diagram != null)
                ResourcesManager.RemoveFile(diagram.ImageLink);

            Directory.CreateDirectory(ResourcesManager.qlistDir);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"qlist_{id}{fileExtension}";
            var filePath = Path.Combine(ResourcesManager.qlistDir, fileName);

            await ResourcesManager.SaveImage(file, filePath);

            var relativePath = ResourcesManager.GetRelativePath(filePath);

            if (diagram != null)
                diagram.ImageLink = relativePath;
            else
            {
                var newDiagram = new DiagramQuestionList
                {
                    ImageLink = relativePath,
                    QlistId = id
                };

                database.DiagramQuestionLists.Add(newDiagram);
            }

            await database.SaveChangesAsync();

            var url = $"{Request.Scheme}://{Request.Host}{relativePath}";

            return Ok(url);
        }

        /// <summary>
        /// Upload a choice list for matching question list.
        /// </summary>
        [HttpPost("choice/{id}")]
        public async Task<IActionResult> AddChoiceList(int id, [FromBody] string choice)
        {
            var qlist = await database.QuestionLists.FindAsync(id);
            if (qlist == null)
                return NotFound("Can't find the question list with id " + id);

            if (qlist.QlistType != "matching")
                return BadRequest("Can't add a choice list to a question list that not the matching type");

            var match = await database.MatchQuestionLists.FirstOrDefaultAsync(m => m.QlistId == id);
            if (match == null)
            {
                var newMatch = new MatchQuestionList
                {
                    ChoiceList = choice,
                    QlistId = id
                };

                database.MatchQuestionLists.Add(newMatch);
            }
            else
                match.ChoiceList = choice;

            await database.SaveChangesAsync();

            return Ok("Update successfully");
        }

        /// <summary>
        /// Get all question lists for a section.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<QuestionListResponseModel>> GetSectionQuestionList([FromQuery] int? sectionId, [FromQuery] string? sectionType)
        {
            if (sectionId == null || sectionType == null)
                return BadRequest("Section id and section type are both required");

            var responseList = new List<QuestionList>();

            if (sectionType == "reading")
            {
                foreach (var qlist in database.QuestionLists.Include(q => q.Rsections))
                {
                    var section = qlist.Rsections.FirstOrDefault();
                    if (section != null && section.RsectionId == sectionId)
                        responseList.Add(qlist);
                }
            }

            if (sectionType == "listening")
            {
                foreach (var qlist in database.QuestionLists.Include(q => q.Lsections))
                {
                    var section = qlist.Lsections.FirstOrDefault();
                    if (section != null && section.LsectionId == sectionId)
                        responseList.Add(qlist);
                }
            }

            return Ok(responseList.Select(qlist => Mapper.QuestionListToResponseModel(qlist)).ToList());
        }

        /// <summary>
        /// Get diagram question list image by question list id.
        /// </summary>
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetDiagramImage(int id)
        {
            var diagram = await database.DiagramQuestionLists.FirstOrDefaultAsync(d => d.QlistId == id);

            if (diagram == null)
                return NotFound("Can't find diagram question list with id " + id);

            return Ok(diagram.ImageLink);
        }

        /// <summary>
        /// Get matching question list choices by question list id.
        /// </summary>
        [HttpGet("choice/{id}")]
        public async Task<IActionResult> GetChoiceList(int id)
        {
            var choice = await database.MatchQuestionLists.FirstOrDefaultAsync(q => q.QlistId == id);

            if (choice == null)
                return NotFound("Can't find matching question list with id " + id);

            return Ok(choice.ChoiceList);
        }
        
        /// <summary>
        /// Update the question list content.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<QuestionListResponseModel>> UpdateQuestionList(int id, [FromBody] string content)
        {
            var qlist = await database.QuestionLists.FindAsync(id);

            if (qlist == null)
                return NotFound("Can't find question list with id " + id);

            qlist.Content = content;

            await database.SaveChangesAsync();
            
            return Ok("Change successfully");
        }
    }
}
