using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Services;

public interface IVisitorService
{
    Task LogVisitAsync(HttpContext context);
    Task<int> GetTodayCountAsync();
    Task<int> GetTotalCountAsync();
    Task<List<VisitorDailyStat>> GetDailyStatsAsync(int days = 30);
    Task<List<VisitorPageStat>> GetTopPagesAsync(int count = 10);
}

public class VisitorDailyStat
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class VisitorPageStat
{
    public string PagePath { get; set; } = "";
    public int Count { get; set; }
}

public class VisitorService : IVisitorService
{
    private readonly ApplicationDbContext _context;

    public VisitorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogVisitAsync(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        // Skip bots/crawlers
        if (!string.IsNullOrEmpty(userAgent) && IsBotUserAgent(userAgent))
            return;

        // Skip static file requests
        var path = httpContext.Request.Path.Value ?? "/";
        if (IsStaticFile(path))
            return;

        // Skip admin pages
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Identity", StringComparison.OrdinalIgnoreCase))
            return;

        var anonymizedIp = AnonymizeIp(ip);

        // Aynı IP'den 30 dakika içinde tekrar ziyaret sayma
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        var recentVisit = await _context.VisitorLogs
            .AnyAsync(v => v.IpAddress == anonymizedIp && v.VisitedAt >= cutoff);

        if (recentVisit)
            return;

        var log = new VisitorLog
        {
            VisitedAt = DateTime.UtcNow,
            IpAddress = anonymizedIp,
            UserAgent = Truncate(userAgent, 500),
            PagePath = Truncate(path, 500),
            Referrer = Truncate(httpContext.Request.Headers.Referer.ToString(), 500)
        };

        _context.VisitorLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTodayCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.VisitorLogs
            .Where(v => v.VisitedAt >= today)
            .Select(v => v.IpAddress)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.VisitorLogs
            .Select(v => v.IpAddress)
            .Distinct()
            .CountAsync();
    }

    public async Task<List<VisitorDailyStat>> GetDailyStatsAsync(int days = 30)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var raw = await _context.VisitorLogs
            .Where(v => v.VisitedAt >= since)
            .Select(v => new { v.VisitedAt, v.IpAddress })
            .ToListAsync();

        return raw
            .GroupBy(v => v.VisitedAt.Date)
            .Select(g => new VisitorDailyStat { Date = g.Key, Count = g.Select(x => x.IpAddress).Distinct().Count() })
            .OrderBy(s => s.Date)
            .ToList();
    }

    public async Task<List<VisitorPageStat>> GetTopPagesAsync(int count = 10)
    {
        return await _context.VisitorLogs
            .Where(v => v.PagePath != null)
            .GroupBy(v => v.PagePath!)
            .Select(g => new VisitorPageStat { PagePath = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .Take(count)
            .ToListAsync();
    }

    private static bool IsBotUserAgent(string ua)
    {
        var bots = new[] { "bot", "crawl", "spider", "slurp", "mediapartners", "googlebot", "bingbot", "yandex", "baidu" };
        var lower = ua.ToLowerInvariant();
        foreach (var bot in bots)
        {
            if (lower.Contains(bot)) return true;
        }
        return false;
    }

    private static bool IsStaticFile(string path)
    {
        var extensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".map", ".webp", ".mp4", ".webm" };
        foreach (var ext in extensions)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string? AnonymizeIp(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return null;
        // Mask last octet for privacy (KVKK)
        var parts = ip.Split('.');
        if (parts.Length == 4)
        {
            parts[3] = "0";
            return string.Join('.', parts);
        }
        // IPv6 — just store first 4 groups
        var v6parts = ip.Split(':');
        if (v6parts.Length > 4)
        {
            return string.Join(':', v6parts.Take(4)) + "::";
        }
        return ip;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
