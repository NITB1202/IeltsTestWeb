using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class QuestionListRequestModel
    {
        [Required(ErrorMessage ="SectionId is required.")]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "SectionType is required.")]
        [RegularExpression("^(reading|listening)$", ErrorMessage = "SectionType must be either 'reading' or 'listening'.")]
        public string SectionType { get; set; } = null!;

        [Required(ErrorMessage = "QuestionListType is required.")]
        [QuestionType]
        public string QuestionListType { get; set; } = null!;

        public string? Content { get; set; }
    }

    public class QuestionTypeAttribute : ValidationAttribute
    {
        private readonly string[] _validTypes = { "multiple_choice", "matching", "true_false", "complete", "diagram" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string strValue && Array.Exists(_validTypes, type => type.Equals(strValue, StringComparison.OrdinalIgnoreCase)))
                return ValidationResult.Success;

            return new ValidationResult($"The field {validationContext.DisplayName} must be one of the following types: {string.Join(", ", _validTypes)}.");
        }
    }
}
