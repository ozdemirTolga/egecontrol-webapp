using EgeControlWebApp.Models;
using Microsoft.AspNetCore.Http;

namespace EgeControlWebApp.Services;

public interface IGalleryService
{
    Task<IEnumerable<GalleryItem>> GetAllAsync();
    Task<IEnumerable<GalleryItem>> GetActiveAsync();
    Task<GalleryItem?> GetByIdAsync(int id);
    Task<GalleryItem> CreateAsync(GalleryItem item, IFormFile file);
    Task<GalleryItem> UpdateAsync(GalleryItem item);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
