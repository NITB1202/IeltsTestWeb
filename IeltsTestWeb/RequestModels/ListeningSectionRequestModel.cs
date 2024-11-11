using Swashbuckle.AspNetCore.Annotations;

namespace IeltsTestWeb.RequestModels
{
    public class ListeningSectionRequestModel
    {
        public int SectionOrder { get; set; }

        public TimeOnly TimeStamp { get; set; }
        
        public string? Transcript { get; set; }
        
        public int SoundId { get; set; }
    }
}
