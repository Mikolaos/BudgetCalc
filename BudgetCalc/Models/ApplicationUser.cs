using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BudgetCalc.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Expense> Expenses { get; set; } = new();
        public List<Earning> Earnings { get; set; } = new();
    }
}
