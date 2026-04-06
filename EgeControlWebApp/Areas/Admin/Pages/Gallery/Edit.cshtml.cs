using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages.Gallery;

[Authorize(Roles = "Admin,SatisTemsilcisi")]
public class EditModel : PageModel
{
    private readonly IGalleryService _galleryService;

    public EditModel(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    [BindProperty]
    public GalleryItem Item { get; set; } = new();

    public IEnumerable<string> ExistingCategories { get; set; } = Enumerable.Empty<string>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _galleryService.GetByIdAsync(id);
        if (item == null) return NotFound();

        Item = item;
        ExistingCategories = await _galleryService.GetCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ExistingCategories = await _galleryService.GetCategoriesAsync();

        if (!ModelState.IsValid)
            return Page();

        await _galleryService.UpdateAsync(Item);
        TempData["StatusMessage"] = $"\"{Item.Title}\" güncellendi.";
        return RedirectToPage("Index");
    }
}
