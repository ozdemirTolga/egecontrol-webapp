using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;
using System.Security.Claims;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,SatisTemsilcisi")]
    public class IndexModel : PageModel
    {
        private readonly IQuoteService _quoteService;

        public IndexModel(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public IEnumerable<Quote> Quotes { get; set; } = new List<Quote>();
        public string SearchTerm { get; set; } = string.Empty;

        public async Task OnGetAsync(string searchTerm)
        {
            SearchTerm = searchTerm ?? string.Empty;
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                Quotes = string.IsNullOrWhiteSpace(SearchTerm)
                    ? await _quoteService.GetAllQuotesAsync()
                    : await _quoteService.SearchQuotesAsync(SearchTerm);
            }
            else
            {
                // SatisTemsilcisi - sadece kendi tekliflerini görsün
                Quotes = string.IsNullOrWhiteSpace(SearchTerm)
                    ? await _quoteService.GetQuotesByUserIdAsync(currentUserId)
                    : await _quoteService.SearchQuotesByUserIdAsync(SearchTerm, currentUserId);
            }
        }
    }
}
