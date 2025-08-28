using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Customers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public CreateModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public Customer Customer { get; set; } = new Customer();

        public IActionResult OnGet()
        {
            Customer.IsActive = true; // Varsayılan olarak aktif
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
                await _customerService.CreateCustomerAsync(Customer);
                TempData["SuccessMessage"] = $"{Customer.CompanyName} başarıyla eklendi.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Müşteri eklenirken bir hata oluştu: " + ex.Message);
                return Page();
            }
        }
    }
}
