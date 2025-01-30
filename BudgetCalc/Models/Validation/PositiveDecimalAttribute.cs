using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetCalc.Models.Validation
{
    public class PositiveDecimalAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Wartość nie może być pusta.");
            }

            if (decimal.TryParse(value.ToString(), out decimal decimalValue) && decimalValue > 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Wartość musi być większa od 0!");


        }
    }
}
