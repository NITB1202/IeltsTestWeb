namespace IeltsTestWeb.RequestModels
{
    public class ListResultDetailsRequestModel
    {
        public int resultId { get; set; }
        public Dictionary<int, string> userAnswers { get; set; } = null!;
        public List<int> questionIds { get; set; } = null!;
    }
}
