using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using EgeControlWebApp.Data;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;
using EgeControlWebApp.Infrastructure.Binding;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Allow large file uploads (100 MB) for gallery
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104_857_600;
});

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

// DataProtection: Anahtarları diske kaydet (app pool recycle'da cookie'ler geçersiz olmasın)
var keysDir = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
Directory.CreateDirectory(keysDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("EgeControlWebApp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Anti-forgery cookie ayarları
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.IsEssential = true;
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
        policy.RequireRole("Admin", "SatisTemsilcisi"));
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
// SMTP credentials: environment variable override (güvenlik için)
builder.Services.PostConfigure<SmtpSettings>(settings =>
{
    var envUser = Environment.GetEnvironmentVariable("SMTP_USER");
    var envPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
    if (!string.IsNullOrEmpty(envUser)) settings.User = envUser;
    if (!string.IsNullOrEmpty(envPass)) settings.Password = envPass;
});
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddSingleton<RateLimitingService>();

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
    app.UseHsts();
}

// Localization: TR
var supportedCultures = new[] { new CultureInfo("tr-TR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// HTTPS yönlendirmesi
app.UseHttpsRedirection();
app.UseStaticFiles();

// Güvenlik header'ları
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Ziyaretçi takip middleware
app.Use(async (context, next) =>
{
    await next();
    // Only log successful page responses (not API, not errors, not static files)
    if (context.Response.StatusCode == 200 
        && context.Response.ContentType?.Contains("text/html") == true)
    {
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var visitorService = scope.ServiceProvider.GetRequiredService<IVisitorService>();
            await visitorService.LogVisitAsync(context);
        }
        catch { /* don't break the request */ }
    }
});

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

        // Seed site settings defaults
        var settingsService = sp.GetRequiredService<ISiteSettingsService>();
        await settingsService.EnsureDefaultsAsync();

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

    // Eski rollerdeki kullanıcıları SatisTemsilcisi rolüne taşı
    string[] oldRoles = { "Manager", "QuoteCreator", "QuoteEditor", "QuoteSender", "Viewer" };
    foreach (var user in userManager.Users.ToList())
    {
        var roles = await userManager.GetRolesAsync(user);
        var hasOldRole = roles.Any(r => oldRoles.Contains(r));
        var hasNewRole = roles.Contains(UserRoles.Admin) || roles.Contains(UserRoles.SatisTemsilcisi);

        if (hasOldRole && !hasNewRole)
        {
            await userManager.AddToRoleAsync(user, UserRoles.SatisTemsilcisi);
        }

        // Eski rolleri kaldır
        var toRemove = roles.Where(r => oldRoles.Contains(r)).ToList();
        if (toRemove.Any())
        {
            await userManager.RemoveFromRolesAsync(user, toRemove);
        }
    }

    // Eski rolleri veritabanından sil
    foreach (var oldRole in oldRoles)
    {
        var role = await roleManager.FindByNameAsync(oldRole);
        if (role != null)
        {
            await roleManager.DeleteAsync(role);
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