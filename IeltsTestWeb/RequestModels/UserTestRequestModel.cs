using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class UserTestRequestModel
    {
        [Required(ErrorMessage = "AccountId is required")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "TestType is required")]
        [RegularExpression("^(general|academic)$", ErrorMessage = "TestType must be either 'general' or 'academic'.")]
        public string TestType { get; set; } = null!;

        [Required(ErrorMessage = "TestSkill is required")]
        [RegularExpression("^(reading|listening)$", ErrorMessage = "TestSkill must be either 'reading' or 'listening'.")]
        public string TestSkill { get; set; } = null!;
    }
}
