using IeltsTestWeb.Models;
using IeltsTestWeb.RequestModels;
using IeltsTestWeb.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class QuestionController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public QuestionController(ieltsDbContext database)
        {
            this.database = database;
        }
        private static QuestionResponseModel QuestionToResponseModel(Question model)
        {
            return new QuestionResponseModel
            {
                QuestionId = model.QuestionId,
                QlistId = model.QlistId,
                Content = model.Content,
                ChoiceList = model.ChoiceList,
                Answer = model.Answer
            };
        }
        
        /// <summary>
        /// Create new question for question list.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<QuestionResponseModel>> CreateNewQuestion([FromBody]QuestionRequestModel request )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var qlist = await database.QuestionLists.FindAsync(request.QlistId);
            if (qlist == null)
                return NotFound("Can't find question list with id " + request.QlistId);

            qlist.Qnum += 1;

            var question = new Question
            {
                QlistId = request.QlistId,
                Content = request.Content,
                ChoiceList = request.ChoiceList,
                Answer = request.Answer
            };

            database.Questions.Add(question);
            
            await database.SaveChangesAsync();

            return Ok(QuestionToResponseModel(question));
        }

        /// <summary>
        /// Get all questions belongs to the question list.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<QuestionResponseModel>>> GetQuestionListQuestion(int id)
        {
            var questions = database.Questions.Where(q => q.QlistId == id);
            var responeList = await questions.Select(q => QuestionToResponseModel(q)).ToListAsync();
            return Ok(responeList);
        }
        
        /// <summary>
        /// Update the question information. 
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<QuestionResponseModel>> UpdateQuestion(int id, [FromBody]UpdateQuestionRequestModel request)
        {
            var question = await database.Questions.FindAsync(id);

            if (question == null)
                return NotFound("Can't find question with id " + id);

            var requestProperties = typeof(UpdateQuestionRequestModel).GetProperties();
            var questionProperties = typeof(Question).GetProperties();

            foreach (var prop in requestProperties)
            {
                var requestValue = prop.GetValue(request);

                // If the value is not null, find corresponding property in question and update
                if (requestValue != null)
                {
                    var questionProp = questionProperties.FirstOrDefault(p => p.Name == prop.Name);
                    if (questionProp != null && questionProp.CanWrite)
                        questionProp.SetValue(question, requestValue);
                }
            }

            await database.SaveChangesAsync();

            return Ok(QuestionToResponseModel(question));
        }
    }
}
