using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages.Gallery;

[Authorize(Roles = "Admin,SatisTemsilcisi")]
public class IndexModel : PageModel
{
    private readonly IGalleryService _galleryService;

    public IndexModel(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    public IEnumerable<GalleryItem> Items { get; set; } = Enumerable.Empty<GalleryItem>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Items = await _galleryService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var item = await _galleryService.GetByIdAsync(id);
        if (item == null) return NotFound();

        item.IsActive = !item.IsActive;
        await _galleryService.UpdateAsync(item);

        StatusMessage = item.IsActive ? $"\"{item.Title}\" aktif edildi." : $"\"{item.Title}\" gizlendi.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _galleryService.GetByIdAsync(id);
        var title = item?.Title ?? "";
        await _galleryService.DeleteAsync(id);
        StatusMessage = $"\"{title}\" silindi.";
        return RedirectToPage();
    }
}
