namespace IeltsTestWeb.ResponseModels
{
    public class QuestionDetailsResponseModel
    {
        public QuestionResponseModel Question { get; set; } = null!;

        public ExplanationResponseModel Explanation { get; set; } = null!;
    }
}
