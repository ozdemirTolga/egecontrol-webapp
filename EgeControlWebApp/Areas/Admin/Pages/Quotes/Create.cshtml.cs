using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using EgeControlWebApp.Services;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Quotes
{
    [Authorize(Roles = "Admin,Manager,QuoteCreator")]
    public class CreateModel : PageModel
    {
        private readonly IQuoteService _quoteService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(IQuoteService quoteService, ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _quoteService = quoteService;
            _customerService = customerService;
            _userManager = userManager;
        }

        [BindProperty]
        public Quote Quote { get; set; } = new Quote();

        public SelectList CustomerSelectList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? customerId)
        {
            await LoadCustomersAsync();
            
            Quote.QuoteDate = DateTime.Now;
            Quote.ValidUntil = DateTime.Now.AddDays(30);
            Quote.VatRate = 20;
            Quote.CreatedBy = User.Identity?.Name ?? "Admin";
            Quote.Currency = "EUR"; // Set default currency
            Quote.Status = QuoteStatus.Draft; // Varsayılan olarak taslak
            
            // Set user ID for tracking
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Quote.CreatedByUserId = user.Id;
            }
            
            // Initialize with empty list for indexing support
            Quote.QuoteItems = new List<QuoteItem>();
            
            if (customerId.HasValue)
            {
                Quote.CustomerId = customerId.Value;
            }
            
            // Varsayılan notlar: KDV bilgisi, teklif geçerlilik süresi ve ödeme şekli
            Quote.Notes = $"• KDV dahil değildir.\n• Teklif geçerlilik süresi: {Quote.ValidUntil:dd.MM.yyyy}.\n• Ödeme şekli: Fatura tarihinden itibaren 30 gün içinde banka havalesi ile.";
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool saveAsDraft = false)
        {
            // Debug: Currency değerini kontrol et
            System.Console.WriteLine($"POST - Currency değeri: '{Quote.Currency}'");
            System.Console.WriteLine($"POST - Request.Form Currency: '{Request.Form["Quote.Currency"]}'");
            
            // Navigation property validation hatalarını temizle
            ModelState.Remove("Quote.Customer");
            
            // QuoteItems ve QuoteItemsList navigation property hatalarını temizle
            var keysToRemove = ModelState.Keys
                .Where(key => key.Contains(".Quote") || key.Contains("Customer"))
                .ToList();
                
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (saveAsDraft)
            {
                Quote.Status = QuoteStatus.Draft;
            }
            else
            {
                Quote.Status = QuoteStatus.Draft; // Her durumda taslak olarak başla
            }

            Quote.CreatedBy = User.Identity?.Name ?? "Admin";

            // Set user ID for tracking
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Quote.CreatedByUserId = user.Id;
            }

            // QuoteItemsList'i QuoteItems'a kopyala
            if (Quote.QuoteItemsList != null && Quote.QuoteItemsList.Any())
            {
                // Debug için gelen veriyi logla
                Console.WriteLine($"Received {Quote.QuoteItemsList.Count} quote items:");
                for (int i = 0; i < Quote.QuoteItemsList.Count; i++)
                {
                    var item = Quote.QuoteItemsList[i];
                    Console.WriteLine($"Item {i}: Name='{item.ItemName}', Qty={item.Quantity}, Price={item.UnitPrice}");
                }

                // Boş öğeleri filtrele ve navigation property'leri temizle
                var validItems = Quote.QuoteItemsList
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemName) && 
                                   item.Quantity > 0 && 
                                   item.UnitPrice >= 0)
                    .ToList();

                Console.WriteLine($"After filtering: {validItems.Count} valid items");

                if (!validItems.Any())
                {
                    ModelState.AddModelError(string.Empty, "En az bir geçerli teklif kalemi eklemelisiniz.");
                    await LoadCustomersAsync();
                    return Page();
                }

                var cleanItems = validItems.Select(item => new QuoteItem
                {
                    ItemName = item.ItemName.Trim(),
                    Description = item.Description?.Trim() ?? string.Empty,
                    Quantity = NormalizeDecimal(item.Quantity),
                    UnitPrice = NormalizeDecimal(item.UnitPrice),
                    Unit = string.IsNullOrWhiteSpace(item.Unit) ? "adet" : item.Unit.Trim(),
                    DiscountPercentage = NormalizeDecimal(item.DiscountPercentage),
                    DiscountAmount = item.DiscountAmount,
                    Total = item.Total,
                    SortOrder = item.SortOrder,
                    QuoteId = 0 // Bu servis tarafından atanacak
                }).ToList();
                
                Console.WriteLine($"Clean items created: {cleanItems.Count}");
                foreach (var item in cleanItems)
                {
                    Console.WriteLine($"Clean item: Name='{item.ItemName}', Qty={item.Quantity}, Price={item.UnitPrice}");
                }
                
                Quote.QuoteItems = cleanItems;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "En az bir teklif kalemi eklemelisiniz.");
                await LoadCustomersAsync();
                return Page();
            }

            // Debug için model durumunu kontrol et
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                    .ToArray();
                
                // Hataları TempData'ya ekle
                TempData["ValidationErrors"] = string.Join("; ", errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
                
                await LoadCustomersAsync();
                return Page();
            }

            try
            {
                await _quoteService.CreateQuoteAsync(Quote);
                
                var statusText = saveAsDraft ? "taslak olarak kaydedildi" : "başarıyla oluşturuldu ve taslak olarak kaydedildi";
                TempData["SuccessMessage"] = $"Teklif {statusText}. E-posta göndermek için teklif detayına gidin.";
                
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Teklif oluşturulurken bir hata oluştu: " + ex.Message);
                TempData["ErrorMessage"] = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                await LoadCustomersAsync();
                return Page();
            }
        }

        private async Task LoadCustomersAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customerOptions = customers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrEmpty(c.ContactPerson) ? c.CompanyName : $"{c.CompanyName} - {c.ContactPerson}"
            }).ToList();
            
            CustomerSelectList = new SelectList(customerOptions, "Value", "Text");
        }

        private static decimal NormalizeDecimal(decimal value)
        {
            // Decimal values are already parsed by model binding, so just return as-is
            // The real normalization happens in the custom model binder or request culture
            return value;
        }
    }
}
