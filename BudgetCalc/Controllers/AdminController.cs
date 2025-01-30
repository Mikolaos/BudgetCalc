using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BudgetCalc.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BudgetCalcDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, BudgetCalcDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var currentUser = await _userManager.GetUserAsync(User);

        if (user == null || currentUser == null)
        {
            return NotFound();
        }

        // Sprawdzamy, czy użytkownik próbuje usunąć samego siebie
        if (user.Id == currentUser.Id)
        {
            TempData["Error"] = "Nie możesz usunąć własnego konta!";
            return RedirectToAction("Users");
        }

        var userExpenses = _context.Expenses.Where(e => e.UserId == user.Id);
        var userEarnings = _context.Earnings.Where(e => e.UserId == user.Id);

        _context.Expenses.RemoveRange(userExpenses);
        _context.Earnings.RemoveRange(userEarnings);
        await _context.SaveChangesAsync();

        await _userManager.DeleteAsync(user);

        return RedirectToAction("Users");
    }

}
