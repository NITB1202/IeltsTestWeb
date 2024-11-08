using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class AccountRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "RoleId is required.")]
        public int RoleId { get; set; }
    }
}
