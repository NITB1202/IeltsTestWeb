using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class UpdateAccountRequestModel
    {
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        //Not checking precision yet
        [Range(1.0, 9.0, ErrorMessage = "Goal must be between 1.0 and 9.0.")]
        public decimal? Goal { get; set; }
    }
}
