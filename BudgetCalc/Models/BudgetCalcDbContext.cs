using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetCalc.Models;

public class BudgetCalcDbContext : IdentityDbContext<ApplicationUser>
{
   public DbSet<Expense> Expenses { get; set; }
   public DbSet<Earning> Earnings { get; set; }

    public BudgetCalcDbContext(DbContextOptions<BudgetCalcDbContext> options)
      : base(options)
   {
      
   }
}