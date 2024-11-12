using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class DetailRequestModel
    {
        [Required(ErrorMessage = "TestId is required.")]
        public int TestId { get; set; }

        [Required(ErrorMessage = "Types is required.")]
        [ValidateList(4, "multiple_choice", "matching", "true_false", "complete", "diagram")]
        public List<string> Types { get; set; } = null!;
    }

    public class ValidateListAttribute : ValidationAttribute
    {
        private readonly int _maxCount;
        private readonly HashSet<string> _allowedValues;

        public ValidateListAttribute(int maxCount, params string[] allowedValues)
        {
            _maxCount = maxCount;
            _allowedValues = new HashSet<string>(allowedValues);
            ErrorMessage = $"The list can have a maximum of {_maxCount} items, and each item must be one of the following: {string.Join(", ", _allowedValues)}.";
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is List<string> list)
            {
                if (list.Count > _maxCount)
                {
                    return new ValidationResult($"The list can have a maximum of {_maxCount} items.");
                }

                foreach (var item in list)
                {
                    if (!_allowedValues.Contains(item))
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }
            }
            return ValidationResult.Success!;
        }
    }
}
