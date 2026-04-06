using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages;

[Authorize(Roles = "Admin")]
public class SiteSettingsModel : PageModel
{
    private readonly ISiteSettingsService _settings;

    [BindProperty]
    public int StatProjects { get; set; }

    [BindProperty]
    public int StatCustomers { get; set; }

    [BindProperty]
    public int StatExperience { get; set; }

    [BindProperty]
    public int StatSupport { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public SiteSettingsModel(ISiteSettingsService settings)
    {
        _settings = settings;
    }

    public async Task OnGetAsync()
    {
        var all = await _settings.GetAllAsync();
        StatProjects = int.TryParse(all.GetValueOrDefault(SiteSetting.StatProjects), out var p) ? p : 50;
        StatCustomers = int.TryParse(all.GetValueOrDefault(SiteSetting.StatCustomers), out var c) ? c : 50;
        StatExperience = int.TryParse(all.GetValueOrDefault(SiteSetting.StatExperience), out var e) ? e : 20;
        StatSupport = int.TryParse(all.GetValueOrDefault(SiteSetting.StatSupport), out var s) ? s : 24;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _settings.SetValueAsync(SiteSetting.StatProjects, StatProjects.ToString(), "Tamamlanan Proje Sayısı");
        await _settings.SetValueAsync(SiteSetting.StatCustomers, StatCustomers.ToString(), "Mutlu Müşteri Sayısı");
        await _settings.SetValueAsync(SiteSetting.StatExperience, StatExperience.ToString(), "Yıllık Deneyim");
        await _settings.SetValueAsync(SiteSetting.StatSupport, StatSupport.ToString(), "Saat Teknik Destek");

        StatusMessage = "Ayarlar başarıyla kaydedildi.";
        return RedirectToPage();
    }
}
