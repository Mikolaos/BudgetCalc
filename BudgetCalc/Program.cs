using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BudgetCalc.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodanie bazy danych (zmieñ na SQL Server, jeœli potrzebujesz trwa³ej bazy)
builder.Services.AddDbContext<BudgetCalcDbContext>(options =>
    options.UseInMemoryDatabase("BudgetCalcDb") // Mo¿esz zmieniæ na UseSqlServer(connectionString)
);

// Konfiguracja Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<BudgetCalcDbContext>()
    .AddDefaultTokenProviders();

// Konfiguracja ustawieñ ciasteczek i autoryzacji
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Sesja wygasa po 30 minutach
    options.SlidingExpiration = false; // Nie przed³u¿a sesji przy aktywnoœci
});
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true; // Wymagaj unikalnego adresu e-mail
    options.SignIn.RequireConfirmedEmail = false; // Wy³¹cz wymaganie potwierdzenia e-maila
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@._-"; // Pozwól na e-mail jako UserName
});



builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Obs³uga sesji
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Tworzenie ról, jeœli nie istniej¹
    string[] roleNames = { "Admin", "User" };
    foreach (var role in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Rola {role} zosta³a utworzona.");
        }
    }

    // Tworzenie domyœlnego administratora
    string adminEmail = "admin@budgetapp.com";
    string adminPassword = "Admin123!";

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser { UserName = "Admin", Email = adminEmail };
        var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
        if (createAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Administrator zosta³ pomyœlnie utworzony.");
        }
        else
        {
            Console.WriteLine("B³¹d podczas tworzenia administratora:");
            foreach (var error in createAdmin.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
    else
    {
        Console.WriteLine("Administrator ju¿ istnieje.");
    }
}


// Obs³uga b³êdów w trybie deweloperskim
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Pomaga w debugowaniu
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Konfiguracja œcie¿ki middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Obs³uga sesji

app.UseAuthentication();
app.UseAuthorization();

// Definicja domyœlnej trasy
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
