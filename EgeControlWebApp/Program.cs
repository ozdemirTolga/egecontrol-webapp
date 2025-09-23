using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // Production'da SQL Server kullan
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Development'da SQLite kullan
        options.UseSqlite(connectionString);
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
    {
        options.SignIn.RequireConfirmedAccount = false; // Admin için email onayını kapatıyoruz
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>() // Role desteği ekliyoruz
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Oturum süresini uzat - 12 saat
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(12); // 12 saat
    options.SlidingExpiration = true; // Kullanıcı aktif oldukça süre yenilenir
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    // Admin sayfalarını koruma altına alıyoruz
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPolicy");
})
.AddMvcOptions(options =>
{
    // Virgül ve nokta ile decimal sayıları doğru algılamak için özel model binder
    options.ModelBinderProviders.Insert(0, new SimpleDecimalModelBinderProvider());
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin", "Manager", "QuoteCreator", "QuoteEditor", "QuoteSender", "Viewer"));
});

// Services
builder.Services.AddHttpContextAccessor(); // HttpContext erişimi için
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Otomatik veritabanı yedekleme servisi
builder.Services.AddHostedService<DatabaseBackupService>();

// IIS/Production altında ANCM (AspNetCoreModuleV2) dinleme adresini atar.
// Geliştirici makinesinde launchSettings.json kullanılır. Burada sabit URL tanımlamıyoruz.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Localization: default to Turkish (tr-TR) so comma decimals are parsed correctly
var supportedCultures = new[] { new CultureInfo("tr-TR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Seed admin user and roles
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await SeedAdminUser(serviceProvider);
}

app.Run();

// Admin kullanıcı ve rol oluşturma
async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Tüm rolleri oluştur
    foreach (var role in UserRoles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Admin kullanıcı oluştur
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

// Simple decimal model binder that handles Turkish comma format
public class SimpleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        // Simple Turkish decimal parsing: 65,32 -> 65.32
        var normalizedValue = value.Replace(",", ".");
        
        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Geçersiz sayı: {value}");
        }

        return Task.CompletedTask;
    }
}

public class SimpleDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
        {
            return new BinderTypeModelBinder(typeof(SimpleDecimalModelBinder));
        }
        return null;
    }
}
