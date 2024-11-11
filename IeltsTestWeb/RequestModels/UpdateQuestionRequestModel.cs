namespace IeltsTestWeb.RequestModels
{
    public class UpdateQuestionRequestModel
    {
        public string? Content { get; set; }

        public string? ChoiceList { get; set; }

        public string? Answer { get; set; } = null!;
    }
}
