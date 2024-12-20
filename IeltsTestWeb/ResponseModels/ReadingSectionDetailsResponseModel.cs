namespace IeltsTestWeb.ResponseModels
{
    public class ReadingSectionDetailsResponseModel
    {
        public ReadingSectionResponseModel Section { get; set; } = null!;
        public List<QuestionListDetailResponseModel> QuestionLists { get; set; } = null!;
    }
}
