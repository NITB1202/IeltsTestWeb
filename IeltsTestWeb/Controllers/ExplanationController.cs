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
    public class ExplanationController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public ExplanationController(ieltsDbContext database)
        {
            this.database = database;
        }
        private static ExplanationResponseModel ExplanationToResponseModel(Explanation model)
        {
            return new ExplanationResponseModel
            {
                ExId = model.ExId,
                Content = model.Content,
                QuestionId = model.QuestionId
            };
        }

        /// <summary>
        /// Create an explanation for the question.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ExplanationResponseModel>> CreateNewExplanation([FromBody] ExplanationRequestModel request)
        {
            if (!await database.Questions.AnyAsync(q => q.QuestionId == request.QuestionId))
                return NotFound("Can't find a question with id " + request.QuestionId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await database.Explanations.AnyAsync(ex => ex.QuestionId == request.QuestionId))
                return BadRequest("The explanation for this question already exists");

            var ex = new Explanation
            {
                Content = request.Content,
                QuestionId = request.QuestionId
            };

            database.Explanations.Add(ex);
            await database.SaveChangesAsync();

            return Ok(ExplanationToResponseModel(ex));
        }
        
        /// <summary>
        /// Get an explanation for the question.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExplanationResponseModel>> GetExplanation(int id)
        {
            var ex = await database.Explanations.FirstOrDefaultAsync(e => e.QuestionId == id);

            if (ex == null)
                return NotFound("Can't find the explanation for the question with id " + id);

            return Ok(ExplanationToResponseModel(ex));
        }
        
        /// <summary>
        /// Update the explanation content.
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<ExplanationResponseModel>> UpdateExplanation(int id, [FromBody] string content)
        {
            var ex = await database.Explanations.FindAsync(id);

            if (ex == null)
                return NotFound("Can't find the explanation for the question with id " + id);

            ex.Content = content;

            await database.SaveChangesAsync();

            return Ok(ExplanationToResponseModel(ex));
        }
    }
}
