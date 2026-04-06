using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Services;

public interface ISiteSettingsService
{
    Task<string> GetValueAsync(string key, string defaultValue = "0");
    Task SetValueAsync(string key, string value, string? description = null);
    Task<Dictionary<string, string>> GetAllAsync();
    Task EnsureDefaultsAsync();
}

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _context;

    public SiteSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetValueAsync(string key, string defaultValue = "0")
    {
        var setting = await _context.SiteSettings.FindAsync(key);
        return setting?.Value ?? defaultValue;
    }

    public async Task SetValueAsync(string key, string value, string? description = null)
    {
        var setting = await _context.SiteSettings.FindAsync(key);
        if (setting == null)
        {
            setting = new SiteSetting { Key = key, Value = value, Description = description };
            _context.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            if (description != null) setting.Description = description;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        return await _context.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task EnsureDefaultsAsync()
    {
        var defaults = new Dictionary<string, (string Value, string Description)>
        {
            [SiteSetting.StatProjects] = ("50", "Tamamlanan Proje Sayısı"),
            [SiteSetting.StatCustomers] = ("50", "Mutlu Müşteri Sayısı"),
            [SiteSetting.StatExperience] = ("20", "Yıllık Deneyim"),
            [SiteSetting.StatSupport] = ("24", "Saat Teknik Destek"),
        };

        foreach (var (key, (value, desc)) in defaults)
        {
            if (!await _context.SiteSettings.AnyAsync(s => s.Key == key))
            {
                _context.SiteSettings.Add(new SiteSetting { Key = key, Value = value, Description = desc });
            }
        }
        await _context.SaveChangesAsync();
    }
}
