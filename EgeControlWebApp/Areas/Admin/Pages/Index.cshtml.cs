using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
    public class IndexModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IQuoteService _quoteService;

        public IndexModel(ICustomerService customerService, IQuoteService quoteService)
        {
            _customerService = customerService;
            _quoteService = quoteService;
        }

        public int TotalCustomers { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public decimal TotalQuoteValue { get; set; }
        public Dictionary<string, decimal> TotalsByCurrency { get; set; } = new();
        public IEnumerable<Quote> RecentQuotes { get; set; } = new List<Quote>();
        public IEnumerable<Customer> RecentCustomers { get; set; } = new List<Customer>();

        public async Task OnGetAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var quotes = await _quoteService.GetAllQuotesAsync();

            TotalCustomers = customers.Count();
            TotalQuotes = quotes.Count();
            PendingQuotes = quotes.Count(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent);
            TotalQuoteValue = quotes.Sum(q => q.TotalAmount);

            // Group totals by currency
            TotalsByCurrency = quotes
                .GroupBy(q => q.Currency ?? "TRY")
                .ToDictionary(g => g.Key, g => g.Sum(q => q.TotalAmount));

            RecentQuotes = quotes.OrderByDescending(q => q.CreatedAt).Take(5);
            RecentCustomers = customers.OrderByDescending(c => c.CreatedAt).Take(5);
        }
    }
}
