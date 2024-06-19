using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Attributes
{
    public class EmailDomainAttribute(params string[] domains) : ValidationAttribute
    {
        private readonly string[] _domains = domains;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string email)
            {
                var domain = email.Split('@').Last();
                if (_domains.Contains(domain))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"Please enter a valid APU email.");
                }
            }
            return new ValidationResult("Invalid email format.");
        }
    }
}
