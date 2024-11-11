using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class QuestionRequestModel
    {
        public string? Content { get; set; }

        public string? ChoiceList { get; set; }

        [Required(ErrorMessage = "Answer is required.")]
        public string Answer { get; set; } = null!;
    }
}
