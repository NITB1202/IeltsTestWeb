using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class UpdateAccountRequestModel
    { 
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        [Range(1.0, 9.0, ErrorMessage = "Goal must be between 1.0 and 9.0."),
        RegularExpression(@"^\d(\.\d)?$", ErrorMessage = "Goal must have at most one decimal place.")]
        public decimal? Goal { get; set; }
    }
}
