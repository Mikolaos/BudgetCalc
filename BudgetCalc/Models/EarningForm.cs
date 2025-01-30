using BudgetCalc.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace BudgetCalc.Models
{
    public class EarningForm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kwota zarobku jest wymagana.")]
        [PositiveDecimal(ErrorMessage = "Kwota musi być dodatnia.")]
        public decimal Value { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("Jednorazowe|Tygodniowe|Miesięczne")]
        public string Interval { get; set; } = "Jednorazowe";

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
