using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;
using EgeControlWebApp.Infrastructure.Binding;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---- LOGGING:  stdout'a düşsün ki IIS/ANCM görebilsin
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// QuestPDF lisansı
QuestPDF.Settings.License = LicenseType.Community;

// ---- CONNECTION STRINGS (güvenli okuma + fallback)
var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection");
// SQLite dosyasını app köküne sabitleyelim (self-contained'da çalışma dizini exe klasörüdür)
if (string.IsNullOrWhiteSpace(sqliteConn))
{
    var dbPath = Path.Combine(AppContext.BaseDirectory, "app.db");
    sqliteConn = $"Data Source={dbPath}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // 1) SQL Server varsa onu dene
    if (!string.IsNullOrWhiteSpace(sqlServerConn))
    {
        options.UseSqlServer(sqlServerConn);
    }
    else
    {
        // 2) Yoksa kesinlikle düşme → SQLite'a geç
        options.UseSqlite(sqliteConn!);
    }
});

// Prod'da dev exception filtresi gereksiz
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPolicy");
})
.AddMvcOptions(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin", "Manager", "QuoteCreator", "QuoteEditor", "QuoteSender", "Viewer"));
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

var app = builder.Build();

// ---- PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // HTTPS varsa HSTS kullan (SSL sertifikası yoksa bu satırı yoruma al)
    // app.UseHsts();
}

// Localization: TR
var supportedCultures = new[] { new CultureInfo("tr-TR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// HTTPS yönlendirmesi (SSL sertifikası yoksa bu satırı yoruma al)
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Sağlık testi: Kestrel ayakta mı?
app.MapGet("/health", () => Results.Ok("OK"));

// ---- DB init/seed (çökerse uygulamayı düşürme)
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        // İstiyorsan otomatik migrate (bağlantı kurulamazsa catch'e düşer)
        await db.Database.MigrateAsync();

        await SeedAdminUser(sp);
    }
    catch (Exception ex)
    {
        // Uygulama ayakta kalsın; sebebi stdout'a yaz:
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "DB init/seed sırasında hata oluştu. Uygulama çalışmaya devam ediyor.");
    }
}

app.Run();

// ---- Seed
static async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in UserRoles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@egecontrol.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            Department = "IT",
            Position = "System Administrator",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }
    }
}