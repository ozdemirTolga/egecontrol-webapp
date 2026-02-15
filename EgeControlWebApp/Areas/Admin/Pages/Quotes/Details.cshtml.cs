using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;
using EmailAttachment = EgeControlWebApp.Services.EmailAttachment;
using Microsoft.Extensions.Options;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;

        public DetailsModel(ApplicationDbContext context, IPdfService pdfService, IEmailService emailService)
        {
            _context = context;
            _pdfService = pdfService;
            _emailService = emailService;
        }

        public Quote Quote { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quote == null)
            {
                return NotFound();
            }

            // Legacy fix: if totals were not computed previously, recompute and persist once
            if (quote.TotalAmount == 0 && quote.SubTotal == 0 && quote.QuoteItems.Any())
            {
                foreach (var item in quote.QuoteItems)
                {
                    var lineSubtotal = item.Quantity * item.UnitPrice;
                    item.DiscountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                    item.Total = lineSubtotal - item.DiscountAmount;
                }

                quote.SubTotal = quote.QuoteItems.Sum(qi => qi.Total);
                quote.VatAmount = quote.SubTotal * (quote.VatRate / 100);
                quote.TotalAmount = quote.SubTotal + quote.VatAmount;

                await _context.SaveChangesAsync();
            }

            Quote = quote;
            return Page();
        }

        public async Task<IActionResult> OnPostSendEmailAsync(int? id, string? to, string pdfType = "detailed")
        {
            if (id == null)
                return NotFound();

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quote == null)
                return NotFound();

            // Debug için log
            Console.WriteLine($"SendEmail Debug - ID: {id}, TO: '{to}', Customer Email: '{quote.Customer?.Email}'");

            // compute totals if needed
            if (quote.TotalAmount == 0 && quote.SubTotal == 0 && quote.QuoteItems.Any())
            {
                foreach (var item in quote.QuoteItems)
                {
                    var lineSubtotal = item.Quantity * item.UnitPrice;
                    item.DiscountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                    item.Total = lineSubtotal - item.DiscountAmount;
                }
                quote.SubTotal = quote.QuoteItems.Sum(qi => qi.Total);
                quote.VatAmount = quote.SubTotal * (quote.VatRate / 100);
                quote.TotalAmount = quote.SubTotal + quote.VatAmount;
                await _context.SaveChangesAsync();
            }

            // E-posta alıcısını belirle - önce to parametresi, sonra müşteri e-postası
            var recipient = !string.IsNullOrWhiteSpace(to?.Trim()) ? to.Trim() : quote.Customer?.Email?.Trim();
            
            // E-posta adresi kontrolü
            if (string.IsNullOrWhiteSpace(recipient))
            {
                var errorMsg = string.IsNullOrWhiteSpace(quote.Customer?.Email) 
                    ? "E-posta gönderilemedi: Bu müşterinin e-posta adresi kayıtlı değil. Lütfen önce müşteri bilgilerini düzenleyin ve e-posta adresi ekleyin."
                    : "E-posta gönderilemedi: E-posta adresi boş. Lütfen geçerli bir e-posta adresi girin.";
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToPage(new { id });
            }

            // E-posta formatı kontrolü
            try
            {
                var addr = new System.Net.Mail.MailAddress(recipient);
                recipient = addr.Address; // Normalize et
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = $"E-posta gönderilemedi: Geçersiz e-posta formatı: '{recipient}'. Lütfen doğru e-posta adresi girin.";
                return RedirectToPage(new { id });
            }

            try
            {
                // PDF tipini belirle
                var selectedPdfType = pdfType.ToLower() == "summary" ? PdfType.Summary : PdfType.Detailed;
                var pdfBytes = await _pdfService.GenerateQuotePdfAsync(quote, selectedPdfType);
                
                // Dosya adında PDF tipini belirt
                var typeText = selectedPdfType == PdfType.Summary ? "Ozet" : "Detayli";
                var fileName = $"Teklif_{quote.QuoteNumber}_{typeText}_{DateTime.Now:yyyyMMdd}.pdf";
                
                var subject = $"Teklif {quote.QuoteNumber} - Ege Otomasyon";
                var pdfTypeDescription = selectedPdfType == PdfType.Summary ? "özet" : "detaylı";
                var body = $@"Merhaba {quote.Customer?.ContactPerson ?? quote.Customer?.CompanyName ?? ""},<br/><br/>" +
                           $"Ek'te {quote.QuoteDate:dd.MM.yyyy} tarihli {quote.QuoteNumber} numaralı teklifimizin {pdfTypeDescription} versiyonunu bulabilirsiniz.<br/><br/>" +
                           $"Saygılarımızla,<br/>Ege Otomasyon";

                var attachments = new List<EmailAttachment>
                {
                    new EmailAttachment(fileName, "application/pdf", pdfBytes)
                };

                // BCC olarak kendi adresimize de gönder
                await _emailService.SendAsync(recipient, subject, body, attachments, cc: null, bcc: "tolga.ozdemir@live.com");
                
                // E-posta başarıyla gönderildikten sonra durumu "Gönderildi" olarak güncelle
                quote.Status = QuoteStatus.Sent;
                quote.UpdatedAt = DateTime.UtcNow;
                quote.LastModifiedBy = User.Identity?.Name ?? "System";
                var currentUserName = User.Identity?.Name ?? "";
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUserName);
                if (currentUser != null)
                {
                    quote.LastModifiedByUserId = currentUser.Id;
                }
                await _context.SaveChangesAsync();
                
                // If pickup directory is enabled, inform where the email was saved
                var smtpOpts = HttpContext.RequestServices.GetService<IOptions<SmtpSettings>>();
                if (smtpOpts?.Value.UsePickupDirectory == true)
                {
                    var pickup = smtpOpts.Value.PickupDirectory;
                    if (string.IsNullOrWhiteSpace(pickup))
                    {
                        pickup = Path.Combine(AppContext.BaseDirectory, "MailDrop");
                    }
                    TempData["SuccessMessage"] = $"Teklif ({pdfTypeDescription}) e-posta dosyaya kaydedildi: {pickup}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Teklif ({pdfTypeDescription}) e-posta ile gönderildi: {recipient}";
                }
            }
            catch (Exception ex)
            {
                // Show more diagnostic info in Development
                var detail = ex.ToString();
                if (detail.Length > 2000) detail = detail.Substring(0, 2000) + "...";
                TempData["ErrorMessage"] = $"E-posta gönderilirken hata oluştu: {ex.Message} | Detay: {detail}";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostGeneratePdfAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quote == null)
            {
                return NotFound();
            }

            try
            {
                // Ensure totals exist before generating PDF
                if (quote.TotalAmount == 0 && quote.SubTotal == 0 && quote.QuoteItems.Any())
                {
                    foreach (var item in quote.QuoteItems)
                    {
                        var lineSubtotal = item.Quantity * item.UnitPrice;
                        item.DiscountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                        item.Total = lineSubtotal - item.DiscountAmount;
                    }

                    quote.SubTotal = quote.QuoteItems.Sum(qi => qi.Total);
                    quote.VatAmount = quote.SubTotal * (quote.VatRate / 100);
                    quote.TotalAmount = quote.SubTotal + quote.VatAmount;
                    await _context.SaveChangesAsync();
                }

                var pdfBytes = await _pdfService.GenerateQuotePdfAsync(quote);
                var fileName = $"Teklif_{quote.QuoteNumber}_{DateTime.Now:yyyyMMdd}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"PDF oluşturulurken hata oluştu: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetGeneratePdfAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quote == null)
            {
                return NotFound();
            }

            try
            {
                // Ensure totals exist before generating PDF
                if (quote.TotalAmount == 0 && quote.SubTotal == 0 && quote.QuoteItems.Any())
                {
                    foreach (var item in quote.QuoteItems)
                    {
                        var lineSubtotal = item.Quantity * item.UnitPrice;
                        item.DiscountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                        item.Total = lineSubtotal - item.DiscountAmount;
                    }

                    quote.SubTotal = quote.QuoteItems.Sum(qi => qi.Total);
                    quote.VatAmount = quote.SubTotal * (quote.VatRate / 100);
                    quote.TotalAmount = quote.SubTotal + quote.VatAmount;
                    await _context.SaveChangesAsync();
                }

                var pdfBytes = await _pdfService.GenerateQuotePdfAsync(quote);
                var fileName = $"Teklif_{quote.QuoteNumber}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"PDF oluşturulurken hata oluştu: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostSaveAsAsync(int id)
        {
            var originalQuote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (originalQuote == null)
            {
                return NotFound();
            }

            try
            {
                // Yeni teklifi oluştur (kopyala)
                var newQuote = new Quote
                {
                    CustomerId = originalQuote.CustomerId,
                    QuoteNumber = await GenerateNewQuoteNumberAsync(),
                    QuoteDate = DateTime.Now,
                    ValidUntil = DateTime.Now.AddDays(30),
                    Title = originalQuote.Title + " (Kopya)",
                    Description = originalQuote.Description,
                    Notes = originalQuote.Notes,
                    VatRate = originalQuote.VatRate,
                    Currency = originalQuote.Currency,
                    Status = QuoteStatus.Draft, // Kopyalanan teklif de taslak olarak başlar
                    CreatedBy = User.Identity?.Name ?? "Admin",
                    CreatedAt = DateTime.Now
                };

                // Kullanıcı ID'sini ayarla
                var currentUserName = User.Identity?.Name ?? "";
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUserName);
                if (user != null)
                {
                    newQuote.CreatedByUserId = user.Id;
                }

                _context.Quotes.Add(newQuote);
                await _context.SaveChangesAsync();

                // Teklif kalemlerini kopyala
                foreach (var originalItem in originalQuote.QuoteItems)
                {
                    var newItem = new QuoteItem
                    {
                        QuoteId = newQuote.Id,
                        ItemName = originalItem.ItemName,
                        Description = originalItem.Description,
                        Quantity = originalItem.Quantity,
                        UnitPrice = originalItem.UnitPrice,
                        Unit = originalItem.Unit,
                        DiscountPercentage = originalItem.DiscountPercentage,
                        DiscountAmount = originalItem.DiscountAmount,
                        Total = originalItem.Total
                    };
                    _context.QuoteItems.Add(newItem);
                }

                await _context.SaveChangesAsync();

                // Toplamları hesapla
                newQuote.SubTotal = newQuote.QuoteItems.Sum(qi => qi.Total);
                newQuote.VatAmount = newQuote.SubTotal * (newQuote.VatRate / 100);
                newQuote.TotalAmount = newQuote.SubTotal + newQuote.VatAmount;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Teklif başarıyla kopyalandı. Yeni teklif numarası: {newQuote.QuoteNumber}";
                return RedirectToPage("./Edit", new { id = newQuote.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Teklif kopyalanırken hata oluştu: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }

        private async Task<string> GenerateNewQuoteNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            
            var lastQuote = await _context.Quotes
                .Where(q => q.CreatedAt.Year == currentYear && q.CreatedAt.Month == currentMonth)
                .OrderByDescending(q => q.CreatedAt)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastQuote != null && !string.IsNullOrWhiteSpace(lastQuote.QuoteNumber))
            {
                var parts = lastQuote.QuoteNumber.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var lastPart = parts.Length > 0 ? parts[^1] : null;
                if (!string.IsNullOrEmpty(lastPart) && int.TryParse(lastPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"EGE-{currentYear:D4}-{currentMonth:00}-{nextNumber:000}";
        }
    }
}
