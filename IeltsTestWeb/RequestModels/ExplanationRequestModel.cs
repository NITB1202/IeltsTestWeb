using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class ExplanationRequestModel
    {
        [Required(ErrorMessage = "QuestionId is required.")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = null!;
    }
}
