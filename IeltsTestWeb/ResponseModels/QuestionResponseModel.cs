namespace IeltsTestWeb.ResponseModels
{
    public class QuestionResponseModel
    {
        public int QuestionId { get; set; }

        public int QlistId { get; set; }

        public string? Content { get; set; }

        public string? ChoiceList { get; set; }

        public string Answer { get; set; } = null!;
    }
}
