using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Pages;

public class GalleryModel : PageModel
{
    private readonly IGalleryService _galleryService;

    public GalleryModel(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    public IEnumerable<GalleryItem> Items { get; set; } = Enumerable.Empty<GalleryItem>();
    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();

    public async Task OnGetAsync()
    {
        Items = await _galleryService.GetActiveAsync();
        Categories = await _galleryService.GetCategoriesAsync();
    }
}
