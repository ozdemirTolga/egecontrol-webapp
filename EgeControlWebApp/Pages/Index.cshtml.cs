using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;
using EgeControlWebApp.Data;

namespace EgeControlWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;

    [BindProperty]
    public ContactMessage Contact { get; set; } = new ContactMessage();

    // Anti-spam: honeypot field (should remain empty)
    [BindProperty]
    public string? Website { get; set; }

    // Anti-spam: timestamp to detect instant bot submissions
    [BindProperty]
    public string? FormToken { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IEmailService emailService, ApplicationDbContext context)
    {
        _logger = logger;
        _emailService = emailService;
        _context = context;
    }

    public void OnGet()
    {
        // Anti-spam: Base64 encoded timestamp
        FormToken = Convert.ToBase64String(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Anti-spam check 1: Honeypot field must be empty
        if (!string.IsNullOrEmpty(Website))
        {
            _logger.LogWarning("Spam tespit edildi (honeypot): {Email} - {Name}", Contact.Email, Contact.Name);
            StatusMessage = "Mesajınız başarıyla gönderildi. Teşekkürler!";
            return RedirectToPage();
        }

        // Anti-spam check 2: Form must be submitted at least 3 seconds after loading
        if (!string.IsNullOrEmpty(FormToken))
        {
            try
            {
                var tokenBytes = Convert.FromBase64String(FormToken);
                var tokenTime = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(tokenBytes, 0));
                var elapsed = DateTimeOffset.UtcNow - tokenTime;
                if (elapsed.TotalSeconds < 3)
                {
                    _logger.LogWarning("Spam tespit edildi (hızlı gönderim {Seconds}s): {Email} - {Name}", elapsed.TotalSeconds, Contact.Email, Contact.Name);
                    StatusMessage = "Mesajınız başarıyla gönderildi. Teşekkürler!";
                    return RedirectToPage();
                }
                // Reject if token is older than 1 hour (stale/replayed form)
                if (elapsed.TotalHours > 1)
                {
                    _logger.LogWarning("Spam tespit edildi (eski form {Hours}h): {Email} - {Name}", elapsed.TotalHours, Contact.Email, Contact.Name);
                    StatusMessage = "Form süresi dolmuş. Lütfen sayfayı yenileyip tekrar deneyin.";
                    return RedirectToPage();
                }
            }
            catch
            {
                // Invalid token - likely bot
                _logger.LogWarning("Spam tespit edildi (geçersiz token): {Email} - {Name}", Contact.Email, Contact.Name);
                StatusMessage = "Mesajınız başarıyla gönderildi. Teşekkürler!";
                return RedirectToPage();
            }
        }
        else
        {
            // No token = bot
            _logger.LogWarning("Spam tespit edildi (token yok): {Email} - {Name}", Contact.Email, Contact.Name);
            StatusMessage = "Mesajınız başarıyla gönderildi. Teşekkürler!";
            return RedirectToPage();
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _logger.LogInformation("İletişim mesajı alındı: {Email} - {Name}", Contact.Email, Contact.Name);

        // Önce veritabanına kaydet (canlı ortamda SQLite yetki sorunlarına karşı try/catch)
        var savedToDb = false;
        try
        {
            _context.ContactMessages.Add(Contact);
            await _context.SaveChangesAsync();
            savedToDb = true;
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "İletişim mesajı veritabanına kaydedilemedi. Email: {Email}, Name: {Name}", Contact.Email, Contact.Name);
            // Kullanıcıya kibar bir bilgilendirme verelim; e-postayı yine de deneyeceğiz
            StatusMessage = "Mesajınız alınmıştır ancak sistem kaydı sırasında sorun oluştu. E-postayla iletmeyi deneyeceğiz.";
        }

        // E-posta göndermeyi dene
        try
        {
            var subject = $"Yeni İletişim Mesajı - {Contact.Name}";
            var body = $@"
                <h3>Yeni İletişim Mesajı</h3>
                <p><strong>Ad:</strong> {Contact.Name}</p>
                <p><strong>E-posta:</strong> {Contact.Email}</p>
                <p><strong>Mesaj:</strong></p>
                <p>{Contact.Message.Replace("\n", "<br>")}</p>
                <p><strong>Tarih:</strong> {Contact.CreatedAt:dd.MM.yyyy HH:mm}</p>
                <hr>
                <p><small>Bu mesaj otomatik olarak gönderilmiştir. Mesaj ID: {Contact.Id}</small></p>
            ";

            await _emailService.SendAsync("tolga.ozdemir@egecontrol.com", subject, body);
            
            // E-posta başarılı, mümkünse veritabanını güncelle
            Contact.EmailSent = true;
            if (savedToDb)
            {
                try
                {
                    _context.ContactMessages.Update(Contact);
                    await _context.SaveChangesAsync();
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "E-posta sonrası veritabanı güncellenemedi. ID: {Id}", Contact.Id);
                }
            }
            
            _logger.LogInformation("İletişim mesajı başarıyla gönderildi: {Email} - ID: {Id}", Contact.Email, Contact.Id);
            StatusMessage = $"Mesajınız başarıyla gönderildi (#{Contact.Id}). En kısa sürede size dönüş yapacağız.";
        }
        catch (Exception ex)
        {
            // E-posta hatası, mümkünse veritabanına kaydet
            Contact.EmailSent = false;
            Contact.EmailError = ex.Message;
            if (savedToDb)
            {
                try
                {
                    _context.ContactMessages.Update(Contact);
                    await _context.SaveChangesAsync();
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "E-posta hatası sonrası veritabanı güncellenemedi. ID: {Id}", Contact.Id);
                }
            }
            
            _logger.LogError(ex, "İletişim mesajı gönderilirken hata oluştu: {Email} - ID: {Id} - {Error}", 
                Contact.Email, Contact.Id, ex.Message);
            
            StatusMessage = $"Mesajınız kaydedildi (#{Contact.Id}) ancak e-posta gönderiminde sorun oluştu. " +
                          "Mesajınızı aldık ve en kısa sürede dönüş yapacağız. " +
                          "Acil durumlar için doğrudan tolga.ozdemir@egecontrol.com adresine yazabilirsiniz.";
        }

        // Formu temizle
    Contact = new ContactMessage();
    return RedirectToPage();
    }
}
