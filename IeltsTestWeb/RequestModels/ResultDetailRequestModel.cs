using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class ResultDetailRequestModel
    {
        [Required(ErrorMessage = "ResultId is required.")]
        public int ResultId { get; set; }

        [Required(ErrorMessage = "QuestionOrder is required.")]
        [Range(1, 40, ErrorMessage = "QuestionOrder must be between 1 and 40.")]
        public int QuestionOrder { get; set; }

        [Required(ErrorMessage = "QuestionId is required.")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "UserAnswer is required.")]
        public string UserAnswer { get; set; } = null!;
    }
}
