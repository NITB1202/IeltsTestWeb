using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class ResultRequestModel
    {
        [Required(ErrorMessage = "AccountId is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "TestId is required.")]
        public int TestId { get; set; }

        [Required(ErrorMessage = "TestAccess is required.")]
        [RegularExpression("^(private|public)$", ErrorMessage = "TestAccess must be either 'private' or 'public'.")]
        public string TestAccess { get; set; } = null!;

        [Required(ErrorMessage = "CompleteTime is required.")]
        public TimeOnly? CompleteTime { get; set; }
    }
}
