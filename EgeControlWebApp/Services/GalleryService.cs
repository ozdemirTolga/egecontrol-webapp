using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Services;

public class GalleryService : IGalleryService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm", ".mov", ".avi" };

    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public GalleryService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IEnumerable<GalleryItem>> GetAllAsync()
    {
        return await _context.GalleryItems
            .OrderBy(g => g.SortOrder)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GalleryItem>> GetActiveAsync()
    {
        return await _context.GalleryItems
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<GalleryItem?> GetByIdAsync(int id)
    {
        return await _context.GalleryItems.FindAsync(id);
    }

    public async Task<GalleryItem> CreateAsync(GalleryItem item, IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("Dosya boyutu 100 MB'dan büyük olamaz.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (AllowedImageExtensions.Contains(ext))
            item.MediaType = "image";
        else if (AllowedVideoExtensions.Contains(ext))
            item.MediaType = "video";
        else
            throw new InvalidOperationException("Desteklenmeyen dosya formatı. JPG, PNG, GIF, WEBP, MP4, WEBM, MOV desteklenir.");

        var uploadsDir = Path.Combine(_env.WebRootPath, "gallery");
        Directory.CreateDirectory(uploadsDir);

        // Unique filename to prevent collisions
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        item.FileName = uniqueName;
        item.FileSize = file.Length;
        item.CreatedAt = DateTime.UtcNow;

        _context.GalleryItems.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<GalleryItem> UpdateAsync(GalleryItem item)
    {
        _context.GalleryItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.GalleryItems.FindAsync(id);
        if (item == null) return false;

        // Delete file from disk
        var filePath = Path.Combine(_env.WebRootPath, "gallery", item.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        if (!string.IsNullOrEmpty(item.ThumbnailFileName))
        {
            var thumbPath = Path.Combine(_env.WebRootPath, "gallery", item.ThumbnailFileName);
            if (File.Exists(thumbPath))
                File.Delete(thumbPath);
        }

        _context.GalleryItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.GalleryItems
            .Where(g => g.Category != null && g.Category != "")
            .Select(g => g.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
