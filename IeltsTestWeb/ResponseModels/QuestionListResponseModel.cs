namespace IeltsTestWeb.ResponseModels
{
    public class QuestionListResponseModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Qnum { get; set; }
    }
}
