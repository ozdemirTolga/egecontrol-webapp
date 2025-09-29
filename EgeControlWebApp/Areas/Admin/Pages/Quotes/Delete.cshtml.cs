using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager")]
    public class DeleteModel : PageModel
    {
        private readonly IQuoteService _quoteService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IQuoteService quoteService, ILogger<DeleteModel> logger)
        {
            _quoteService = quoteService;
            _logger = logger;
        }

        [BindProperty]
        public Quote Quote { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _quoteService.GetQuoteByIdAsync(id.Value);

            if (quote == null)
            {
                return NotFound();
            }

            Quote = quote;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Geçersiz teklif ID'si.";
                return RedirectToPage("./Index");
            }

            try
            {
                var quote = await _quoteService.GetQuoteByIdAsync(id.Value);
                
                if (quote == null)
                {
                    TempData["ErrorMessage"] = "Silinecek teklif bulunamadı.";
                    return RedirectToPage("./Index");
                }

                var result = await _quoteService.DeleteQuoteAsync(id.Value);

                if (result)
                {
                    _logger.LogInformation("Quote {QuoteId} ({QuoteNumber}) deleted by user {UserId}", 
                        id.Value, quote.QuoteNumber, User.Identity?.Name);
                    
                    TempData["SuccessMessage"] = $"Teklif {quote.QuoteNumber} başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Teklif silinirken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quote {QuoteId}", id.Value);
                TempData["ErrorMessage"] = "Teklif silinirken beklenmeyen bir hata oluştu.";
            }

            return RedirectToPage("./Index");
        }
    }
}