using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;
using System.Security.Claims;

namespace EgeControlWebApp.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin,SatisTemsilcisi")]
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
        public bool IsAdmin { get; set; }

        public async Task OnGetAsync()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            IsAdmin = User.IsInRole("Admin");

            IEnumerable<Quote> quotes;
            if (IsAdmin)
            {
                quotes = await _quoteService.GetAllQuotesAsync();
            }
            else
            {
                // SatisTemsilcisi - sadece kendi tekliflerini görsün
                quotes = await _quoteService.GetQuotesByUserIdAsync(currentUserId);
            }

            var customers = await _customerService.GetAllCustomersAsync();

            if (IsAdmin)
            {
                TotalCustomers = customers.Count();
            }
            else
            {
                // SatisTemsilcisi müşteri sayısını kendi tekliflerinden hesaplasın
                TotalCustomers = quotes.Select(q => q.CustomerId).Distinct().Count();
            }

            TotalQuotes = quotes.Count();
            PendingQuotes = quotes.Count(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent);
            TotalQuoteValue = quotes.Sum(q => q.TotalAmount);

            // Group totals by currency
            TotalsByCurrency = quotes
                .GroupBy(q => q.Currency ?? "TRY")
                .ToDictionary(g => g.Key, g => g.Sum(q => q.TotalAmount));

            RecentQuotes = quotes.OrderByDescending(q => q.CreatedAt).Take(5);
            
            if (IsAdmin)
            {
                RecentCustomers = customers.OrderByDescending(c => c.CreatedAt).Take(5);
            }
            else
            {
                // SatisTemsilcisi - sadece kendi tekliflerindeki müşterileri görsün
                var customerIds = quotes.Select(q => q.CustomerId).Distinct().ToHashSet();
                RecentCustomers = customers.Where(c => customerIds.Contains(c.Id)).OrderByDescending(c => c.CreatedAt).Take(5);
            }
        }
    }
}
