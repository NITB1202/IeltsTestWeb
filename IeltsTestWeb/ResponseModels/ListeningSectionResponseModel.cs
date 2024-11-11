namespace IeltsTestWeb.ResponseModels
{
    public class ListeningSectionResponseModel
    {
        public int Id { get; set; }

        public int SectionOrder { get; set; }

        public TimeOnly TimeStamp { get; set; }

        public string? Transcript { get; set; }

        public int SoundId { get; set; }
    }
}
