using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
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
            
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Quotes = await _quoteService.GetAllQuotesAsync();
            }
            else
            {
                Quotes = await _quoteService.SearchQuotesAsync(SearchTerm);
            }
        }
    }
}
