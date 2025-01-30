using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BudgetCalc.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BudgetCalc.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BudgetCalcDbContext _context;

    public HomeController(UserManager<ApplicationUser> userManager, BudgetCalcDbContext context)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Expenses()
    {
        var userId = _userManager.GetUserId(User); // Pobranie ID zalogowanego u¿ytkownika
        var userExpenses = _context.Expenses.Where(e => e.UserId == userId).ToList(); // Filtrujemy dane prostym linq

        var totalExpenses = userExpenses.Sum(x => x.Value);
        ViewBag.Expenses = totalExpenses;

        return View(userExpenses);
    }

    public IActionResult AddAndModifyExpense(int? id)
    {
        if (id.HasValue)
        {
            var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
            if (expenseInDb != null)
            {
                var expenseForm = new ExpenseForm
                {
                    Id = id.Value,
                    Value = expenseInDb.Value,
                    Description = expenseInDb.Description,
                    Date = expenseInDb.Date,
                };

                return View(expenseForm);
            }
        }
        return View(new ExpenseForm());
    }

    [HttpPost]
    public IActionResult AddAndModifyExpenseForm(ExpenseForm model)
    {
        var userId = _userManager.GetUserId(User); // Przypisanie id u¿ytkownika

        if (!ModelState.IsValid)  // Sprawdzamy, czy model jest poprawny
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // Wyœwietlanie b³êdów w konsoli
            }
            return View("AddAndModifyExpense", model);
        }


        var expense = new Expense()
        {
            Id = model.Id,
            UserId = userId,
            Value = model.Value,
            Description = model.Description,
            Date = model.Date,
        };

        if (model.Id == 0)
        {
            _context.Expenses.Add(expense);
        }
        else
        {
            _context.Expenses.Update(expense);
        }

        _context.SaveChanges();
        return RedirectToAction("Expenses");
    }


    public IActionResult DeleteExpense(int id)
    {
        var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
        if (expenseInDb != null)
        {
            _context.Expenses.Remove(expenseInDb);
            _context.SaveChanges();
        }
        return RedirectToAction("Expenses");
    }

    public IActionResult Earnings()
    {
        var userId = _userManager.GetUserId(User);
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var userEarnings = _context.Earnings
            .Where(e => e.UserId == userId && e.Date.Year == currentYear && e.Date.Month == currentMonth)
            .ToList();  //linq do filtracji

        var totalMonthlyEarnings = userEarnings.Sum(x =>
            x.Interval == "Tygodniowe"
                ? x.Value * (DateTime.DaysInMonth(currentYear, currentMonth) / 7.0m)
                : x.Value);

        ViewBag.TotalMonthlyEarnings = totalMonthlyEarnings;
        return View(userEarnings);
    }

    public IActionResult AddAndModifyEarning(int? id)
    {
        if (id.HasValue)
        {
            var earningInDb = _context.Earnings.SingleOrDefault(earning => earning.Id == id);
            if (earningInDb != null)
            {
                var earningForm = new EarningForm
                {
                    Id = id.Value,
                    Value = earningInDb.Value,
                    Description = earningInDb.Description,
                    Interval = earningInDb.Interval,
                    Date = earningInDb.Date,
                };

                return View(earningForm);
            }
        }
        return View(new EarningForm());
    }

    [HttpPost]
    public IActionResult AddAndModifyEarningForm(EarningForm model)
    {
        var userId = _userManager.GetUserId(User);

        if (!ModelState.IsValid)  // Sprawdzamy, czy model jest poprawny
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // Wyœwietlanie b³êdów w konsoli
            }
            return View("AddAndModifyEarning", model); // Zwracamy widok z b³êdami
        }

        var earning = new Earning
        {
            Id = model.Id,
            UserId = userId,
            Value = model.Value,
            Description = model.Description,
            Interval = model.Interval,
            Date = model.Date,
        };

        if (model.Id == 0)
        {
            _context.Earnings.Add(earning);
        }
        else
        {
            _context.Earnings.Update(earning);
        }

        _context.SaveChanges();
        return RedirectToAction("Earnings");
    }


    public IActionResult DeleteEarning(int id)
    {
        var earningInDb = _context.Earnings.SingleOrDefault(earning => earning.Id == id);
        if (earningInDb != null)
        {
            _context.Earnings.Remove(earningInDb);
            _context.SaveChanges();
        }
        return RedirectToAction("Earnings");
    }

    public IActionResult Summary()
    {
        var userId = _userManager.GetUserId(User);
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var monthlyEarnings = _context.Earnings
            .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
            .Sum(e => e.Interval == "Tygodniowe" ? e.Value * (DateTime.DaysInMonth(currentYear, currentMonth) / 7.0m) : e.Value);

        var monthlyExpenses = _context.Expenses
            .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
            .Sum(e => e.Value);

        var budget = monthlyEarnings - monthlyExpenses;

        ViewBag.MonthlyEarnings = monthlyEarnings;
        ViewBag.MonthlyExpenses = monthlyExpenses;
        ViewBag.Budget = budget;

        return View();
    }


    public IActionResult GenerateSummaryChart()
    {
        var userId = _userManager.GetUserId(User);

        var expensesByMonth = _context.Expenses
            .Where(e => e.UserId == userId)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                TotalExpenses = g.Sum(e => e.Value),
                TotalEarnings = 0m
            })
            .ToList();

        var earningsByMonth = _context.Earnings
            .Where(e => e.UserId == userId)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                TotalExpenses = 0m,
                TotalEarnings = g.Sum(e => e.Value)
            })
            .ToList();

        var combinedData = expensesByMonth
            .Concat(earningsByMonth)
            .GroupBy(x => x.Month)
            .Select(g => new
            {
                Month = g.Key,
                TotalExpenses = g.Sum(x => x.TotalExpenses),
                TotalEarnings = g.Sum(x => x.TotalEarnings)
            })
            .OrderBy(x => x.Month)
            .ToList();

        int width = 800;
        int height = 400;
        int margin = 50;

        using (var bitmap = new Bitmap(width, height))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Ustal zakresy dla osi
                graphics.Clear(Color.White);
                decimal maxValue = combinedData.Any()
                ? combinedData.Max(d => Math.Max(d.TotalExpenses, d.TotalEarnings))
                : 1m; // Zapobiega dzieleniu przez 0 i b³êdom w rysowaniu

                if (maxValue == 0)
                    maxValue = 1m;

                int count = combinedData.Count;
                int barWidth = count > 0 ? (width - 2 * margin) / (count * 2) : width - 2 * margin;

                // Rysowanie osi X i Y
                var font = new Font("Arial", 10);
                var pen = new Pen(Color.Black);

                // Oœ Y
                graphics.DrawLine(pen, margin, margin, margin, height - margin);

                // Oœ X
                graphics.DrawLine(pen, margin, height - margin, width - margin, height - margin);

                // Etykiety osi Y
                for (int i = 0; i <= 5; i++)
                {
                    int y = height - margin - i * (height - 2 * margin) / 5;
                    decimal value = maxValue * i / 5;
                    graphics.DrawString(value.ToString("C"), font, Brushes.Black, 5, y - 10);
                    graphics.DrawLine(Pens.Gray, margin, y, width - margin, y); // Linie pomocnicze
                }

                // S³upki i etykiety osi X
                for (int i = 0; i < combinedData.Count; i++)
                {
                    var data = combinedData[i];
                    int x = margin + i * 2 * barWidth;

                    // Rysowanie s³upków
                    int expenseHeight = (int)((data.TotalExpenses / maxValue) * (height - 2 * margin));
                    int earningHeight = (int)((data.TotalEarnings / maxValue) * (height - 2 * margin));

                    graphics.FillRectangle(Brushes.Red, x, height - margin - expenseHeight, barWidth, expenseHeight);
                    graphics.FillRectangle(Brushes.Green, x + barWidth, height - margin - earningHeight, barWidth, earningHeight);

                    // Etykiety miesiêcy
                    string monthLabel = data.Month.ToString("MMM yyyy"); // Skrót miesi¹ca + rok
                    graphics.DrawString(monthLabel, font, Brushes.Black, x, height - margin + 5);
                }
            }

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return File(stream.ToArray(), "image/png");
            }
        }
    }

}
