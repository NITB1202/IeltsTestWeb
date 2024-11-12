namespace IeltsTestWeb.ResponseModels
{
    public class ResultDetailResponseModel
    {
        public int DetailId { get; set; }

        public int ResultId { get; set; }

        public int QuestionOrder { get; set; }

        public int QuestionId { get; set; }

        public string UserAnswer { get; set; } = null!;

        public string QuestionState { get; set; } = null!;
    }
}
