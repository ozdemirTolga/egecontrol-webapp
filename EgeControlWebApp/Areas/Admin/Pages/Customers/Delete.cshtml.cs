using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Customers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            Customer = customer;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                // Müşteriyle ilişkili teklifler var mı kontrol et
                var hasQuotes = await _context.Quotes.AnyAsync(q => q.CustomerId == customer.Id);
                
                if (hasQuotes)
                {
                    // Müşteriyi silmek yerine deaktif et
                    customer.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{customer.CompanyName} müşterisi deaktif edildi (teklifleri olduğu için silinemedi).";
                }
                else
                {
                    // Müşteriyi tamamen sil
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{customer.CompanyName} müşterisi başarıyla silindi.";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
