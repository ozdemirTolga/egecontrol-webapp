using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Models;

public class GalleryItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [StringLength(200)]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ThumbnailFileName { get; set; }

    [Required]
    [StringLength(20)]
    public string MediaType { get; set; } = "image"; // "image" or "video"

    [StringLength(100)]
    [Display(Name = "Kategori")]
    public string? Category { get; set; }

    public long FileSize { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UploadedByUserId { get; set; }

    [Display(Name = "Yükleyen")]
    public string? UploadedByUserName { get; set; }
}
