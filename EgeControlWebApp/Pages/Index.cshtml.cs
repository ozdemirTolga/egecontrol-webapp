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

    }

    public async Task<IActionResult> OnPostAsync()
    {
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
