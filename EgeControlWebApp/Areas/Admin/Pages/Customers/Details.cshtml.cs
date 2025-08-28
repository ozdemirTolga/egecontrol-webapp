using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Customers
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Customer Customer { get; set; } = default!;
        public List<Quote> Quotes { get; set; } = new();
        public int TotalQuotes { get; set; }
        public int AcceptedQuotes { get; set; }
        public decimal TotalQuoteValue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Quotes)
                .ThenInclude(q => q.QuoteItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            Customer = customer;
            Quotes = customer.Quotes.OrderByDescending(q => q.CreatedAt).ToList();
            
            // Calculate statistics
            TotalQuotes = Quotes.Count;
            AcceptedQuotes = Quotes.Count(q => q.Status == QuoteStatus.Accepted);
            TotalQuoteValue = Quotes.Sum(q => q.TotalAmount);

            return Page();
        }
    }
}
