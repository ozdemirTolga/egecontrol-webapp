using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages.Gallery;

[Authorize(Roles = "Admin,SatisTemsilcisi")]
[RequestSizeLimit(104_857_600)] // 100 MB
public class UploadModel : PageModel
{
    private readonly IGalleryService _galleryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UploadModel(IGalleryService galleryService, UserManager<ApplicationUser> userManager)
    {
        _galleryService = galleryService;
        _userManager = userManager;
    }

    [BindProperty]
    public GalleryItem Item { get; set; } = new();

    public IEnumerable<string> ExistingCategories { get; set; } = Enumerable.Empty<string>();

    public async Task OnGetAsync()
    {
        ExistingCategories = await _galleryService.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        ExistingCategories = await _galleryService.GetCategoriesAsync();

        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Lütfen bir dosya seçin.");
            return Page();
        }

        // Remove FileName validation since it's set by service
        ModelState.Remove("Item.FileName");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var user = await _userManager.GetUserAsync(User);
            Item.UploadedByUserId = user?.Id;
            Item.UploadedByUserName = user?.FirstName != null ? $"{user.FirstName} {user.LastName}" : user?.Email;

            await _galleryService.CreateAsync(Item, file);

            TempData["StatusMessage"] = $"\"{Item.Title}\" başarıyla yüklendi.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
