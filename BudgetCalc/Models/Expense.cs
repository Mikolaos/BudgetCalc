using BudgetCalc.Models.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetCalc.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Dodane pole
        public ApplicationUser User { get; set; } // Relacja do użytkownika

        [Required(ErrorMessage = "Kwota wydatku jest wymagana.")]
        [PositiveDecimal(ErrorMessage = "Kwota musi być dodatnia.")]
        public decimal Value { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;
    }


}


