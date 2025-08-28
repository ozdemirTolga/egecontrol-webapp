using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages.Customers
{
    [Authorize]
    public class CustomerEditModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public CustomerEditModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public int TotalQuotes { get; set; }
        public int AcceptedQuotes { get; set; }
        public decimal TotalQuoteValue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }
            Customer = customer;
            
            // İstatistikleri yükle
            var quotes = await _customerService.GetCustomerQuotesAsync(id.Value);
            TotalQuotes = quotes.Count();
            AcceptedQuotes = quotes.Count(q => q.Status == QuoteStatus.Accepted);
            TotalQuoteValue = quotes.Where(q => q.Status == QuoteStatus.Accepted).Sum(q => q.TotalAmount);
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _customerService.UpdateCustomerAsync(Customer);
                TempData["SuccessMessage"] = "Müşteri başarıyla güncellendi.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Müşteri güncellenirken bir hata oluştu: " + ex.Message);
                return Page();
            }
        }
    }
}
