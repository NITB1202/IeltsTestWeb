using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class TestRequestModel
    {
        [Required(ErrorMessage = "TestType is required.")]
        [RegularExpression("^(general|academic)$", ErrorMessage = "TestType must be either 'general' or 'academic'.")]
        public string TestType { get; set; } = null!;

        [Required(ErrorMessage = "TestSkill is required.")]
        [RegularExpression("^(reading|listening)$", ErrorMessage = "TestSkill must be either 'reading' or 'listening'.")]
        public string TestSkill { get; set; } = null!;

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "MonthEdition is required.")]
        [Range(1, 12, ErrorMessage = "MonthEdition must be between 1 and 12.")]
        public int MonthEdition { get; set; }

        [Required(ErrorMessage = "YearEdition is required.")]
        [Range(1, 3000, ErrorMessage = "YearEdition must be greater than 0.")]
        public int YearEdition { get; set; }
    }
}
