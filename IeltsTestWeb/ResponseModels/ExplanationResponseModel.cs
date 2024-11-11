namespace IeltsTestWeb.ResponseModels
{
    public class ExplanationResponseModel
    {
        public int ExId { get; set; }

        public string Content { get; set; } = null!;

        public int QuestionId { get; set; }
    }
}
