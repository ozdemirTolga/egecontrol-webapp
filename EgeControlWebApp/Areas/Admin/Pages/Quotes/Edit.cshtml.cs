using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager,QuoteEditor,QuoteCreator")]
    public class EditModel : PageModel
    {
        private readonly IQuoteService _quoteService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(IQuoteService quoteService, ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _quoteService = quoteService;
            _customerService = customerService;
            _userManager = userManager;
        }

        [BindProperty]
        public Quote Quote { get; set; } = default!;

        public SelectList CustomerOptions { get; set; } = default!;

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

            // Check authorization - QuoteCreator can only edit their own quotes
            // Admin, Manager, QuoteEditor can edit all quotes
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("QuoteCreator") && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("QuoteEditor"))
            {
                if (quote.CreatedByUserId != currentUserId)
                {
                    return Forbid(); // Return 403 Forbidden
                }
            }

            Quote = quote;
            // Convert QuoteItems to List for indexing support
            Quote.QuoteItemsList = Quote.QuoteItems.ToList();
            await LoadCustomerOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check authorization - QuoteCreator can only edit their own quotes
            // Admin, Manager, QuoteEditor can edit all quotes
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("QuoteCreator") && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("QuoteEditor"))
            {
                var existingQuote = await _quoteService.GetQuoteByIdAsync(Quote.Id);
                if (existingQuote == null || existingQuote.CreatedByUserId != currentUserId)
                {
                    return Forbid(); // Return 403 Forbidden
                }
            }

            // Clean navigation-related validation noise
            ModelState.Remove("Quote.Customer");
            var keysToRemove = ModelState.Keys
                .Where(k => k.Contains(".Quote") || k.Contains("Customer"))
                .ToList();
            foreach (var key in keysToRemove) ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                await LoadCustomerOptions();
                return Page();
            }

            // Map QuoteItemsList to a clean QuoteItems collection
            if (Quote.QuoteItemsList != null && Quote.QuoteItemsList.Any())
            {
                // Boş veya geçersiz kalemleri filtrele
                var validItems = Quote.QuoteItemsList
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemName) && 
                                   item.Quantity > 0 && 
                                   item.UnitPrice >= 0)
                    .ToList();

                if (!validItems.Any())
                {
                    ModelState.AddModelError(string.Empty, "En az bir geçerli teklif kalemi eklemelisiniz.");
                    await LoadCustomerOptions();
                    return Page();
                }

                var cleanItems = validItems.Select(item => new QuoteItem
                {
                    Id = item.Id,
                    ItemName = item.ItemName.Trim(),
                    Description = item.Description?.Trim() ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Unit = string.IsNullOrWhiteSpace(item.Unit) ? "Adet" : item.Unit.Trim(),
                    DiscountPercentage = item.DiscountPercentage,
                    DiscountAmount = item.DiscountAmount,
                    Total = item.Total,
                    SortOrder = item.SortOrder,
                    QuoteId = Quote.Id
                }).ToList();
                Quote.QuoteItems = cleanItems;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "En az bir teklif kalemi eklemelisiniz.");
                await LoadCustomerOptions();
                return Page();
            }

            try
            {
                var currentUser = await _userManager.FindByIdAsync(currentUserId ?? "");
                var userName = currentUser?.FullName ?? User.Identity?.Name ?? "Unknown";
                
                await _quoteService.UpdateQuoteAsync(Quote, currentUserId, userName);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuoteExists(Quote.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = "Teklif başarıyla güncellendi.";
            return RedirectToPage("./Details", new { id = Quote.Id });
        }

        private bool QuoteExists(int id)
        {
            // Service already checks existence as part of update; keep lightweight check here if needed.
            return true;
        }

        private async Task LoadCustomerOptions()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customerOptions = customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrEmpty(c.ContactPerson) ? c.CompanyName : $"{c.CompanyName} - {c.ContactPerson}"
            }).ToList();
            
            CustomerOptions = new SelectList(customerOptions, "Value", "Text");
        }
    }
}
