using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Customers
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator,QuoteEditor,QuoteSender,Viewer")]
    public class IndexModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public IndexModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
        public string SearchTerm { get; set; } = string.Empty;

        public async Task OnGetAsync(string searchTerm)
        {
            SearchTerm = searchTerm ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Customers = await _customerService.GetAllCustomersAsync();
            }
            else
            {
                Customers = await _customerService.SearchCustomersAsync(SearchTerm);
            }
        }
    }
}
