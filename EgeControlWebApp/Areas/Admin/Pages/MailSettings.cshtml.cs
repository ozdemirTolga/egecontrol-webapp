using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin,SatisTemsilcisi")]
    public class MailSettingsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public MailSettingsModel(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public bool HasSmtpPassword { get; set; }
        public string SmtpHost { get; set; } = string.Empty;

        [BindProperty]
        public string? SmtpPassword { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            UserEmail = user.Email ?? "";
            UserFullName = user.FullName;
            HasSmtpPassword = !string.IsNullOrWhiteSpace(user.SmtpPassword);
            SmtpHost = _configuration["Smtp:Host"] ?? "mail.egecontrol.com";

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(SmtpPassword))
            {
                StatusMessage = "Şifre boş olamaz.";
                return RedirectToPage();
            }

            user.SmtpPassword = SmtpPassword;
            var result = await _userManager.UpdateAsync(user);

            StatusMessage = result.Succeeded
                ? "SMTP şifreniz başarıyla kaydedildi."
                : "Kaydetme sırasında hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTestAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(user.SmtpPassword))
            {
                StatusMessage = "Önce SMTP şifrenizi kaydedin.";
                return RedirectToPage();
            }

            try
            {
                var senderInfo = new SenderInfo
                {
                    Email = user.Email!,
                    DisplayName = user.FullName,
                    SmtpPassword = user.SmtpPassword
                };

                await _emailService.SendAsUserAsync(
                    senderInfo,
                    user.Email!,
                    "Test E-postası - Ege Control",
                    $"<p>Bu bir test e-postasıdır.</p><p>Gönderen: {user.FullName} ({user.Email})</p><p>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>"
                );

                StatusMessage = $"Test e-postası başarıyla gönderildi: {user.Email}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Test başarısız: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
