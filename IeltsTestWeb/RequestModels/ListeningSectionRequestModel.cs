using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class ListeningSectionRequestModel
    {
        public int SectionOrder { get; set; }

        public TimeOnly TimeStamp { get; set; }
        
        public string? Transcript { get; set; }

        [Required(ErrorMessage = "Sound id is required")]
        public int SoundId { get; set; }
    }
}
