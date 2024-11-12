using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class ReadingSectionRequestModel
    {
        [Required(ErrorMessage = "TestId is required")]
        public int TestId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
