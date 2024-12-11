namespace IeltsTestWeb.ResponseModels
{
    public class ReadingSectionResponseModel
    {
        public int Id { get; set; }

        public string? ImageLink { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int TestId { get; set; }

        public int QuestionNum { get; set; }
    }
}
