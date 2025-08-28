using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
    public class DownloadPdfModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public DownloadPdfModel(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> OnGetAsync(int id, string type = "detailed")
        {
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound();
            }

            // Ensure totals exist before generating PDF
            if ((quote.TotalAmount == 0 || quote.SubTotal == 0) && quote.QuoteItems.Any())
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

            // PDF tipini belirle
            var pdfType = type.ToLower() == "summary" ? PdfType.Summary : PdfType.Detailed;
            var pdfBytes = await _pdfService.GenerateQuotePdfAsync(quote, pdfType);
            
            // Dosya adında PDF tipini belirt
            var typeText = pdfType == PdfType.Summary ? "Ozet" : "Detayli";
            var fileName = $"Teklif_{quote.QuoteNumber}_{typeText}_{DateTime.Now:yyyyMMdd}.pdf";
            
            // Content-Disposition header'ını attachment olarak ayarla
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }
    }
}
