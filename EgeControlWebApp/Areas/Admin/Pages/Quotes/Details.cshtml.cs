using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;
using EmailAttachment = EgeControlWebApp.Services.EmailAttachment;

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
                
                var subject = $"Teklif {quote.QuoteNumber} - EGE CONTROL";
                var pdfTypeDescription = selectedPdfType == PdfType.Summary ? "özet" : "detaylı";
                var body = $@"Merhaba {quote.Customer?.ContactPerson ?? quote.Customer?.CompanyName ?? ""},<br/><br/>" +
                           $"Ek'te {quote.QuoteDate:dd.MM.yyyy} tarihli {quote.QuoteNumber} numaralı teklifimizin {pdfTypeDescription} versiyonunu bulabilirsiniz.<br/><br/>" +
                           $"Saygılarımızla,<br/>EGE CONTROL";

                var attachments = new List<EmailAttachment>
                {
                    new EmailAttachment(fileName, "application/pdf", pdfBytes)
                };

                await _emailService.SendAsync(recipient, subject, body, attachments);
                TempData["SuccessMessage"] = $"Teklif ({pdfTypeDescription}) e-posta ile gönderildi: {recipient}";
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
    }
}
