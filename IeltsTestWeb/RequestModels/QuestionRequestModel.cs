using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class QuestionRequestModel
    {
        [Required(ErrorMessage = "QlistId is required.")]
        public int QlistId { get; set; }
        public string? Content { get; set; }
        public string? ChoiceList { get; set; }

        [Required(ErrorMessage = "Answer is required.")]
        public string Answer { get; set; } = null!;
    }
}
