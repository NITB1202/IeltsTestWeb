namespace IeltsTestWeb.ResponseModels
{
    public class ListeningSectionDetailsResponseModel
    {
        public ListeningSectionResponseModel Section { get; set; } = null!;
        public List<QuestionListDetailResponseModel> QuestionLists { get; set; } = null!;
    }
}
