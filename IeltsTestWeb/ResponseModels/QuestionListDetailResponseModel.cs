namespace IeltsTestWeb.ResponseModels
{
    public class QuestionListDetailResponseModel
    {
        public QuestionListResponseModel questionList { get; set; } = null!;
        public List<QuestionDetailsResponseModel> questions { get; set; } = null!;
    }
}
