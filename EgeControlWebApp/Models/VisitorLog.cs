using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Models;

public class VisitorLog
{
    public int Id { get; set; }

    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(500)]
    public string? PagePath { get; set; }

    [MaxLength(500)]
    public string? Referrer { get; set; }
}
