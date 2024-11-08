using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class VerifyRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } =  null!;

        [Required(ErrorMessage = "Verification code is required.")]
        public string VerificationCode { get; set; } = null!;
    }
}
