using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Pages;

[Authorize(Roles = "Admin")]
public class VisitorStatsModel : PageModel
{
    private readonly IVisitorService _visitorService;

    public int TodayCount { get; set; }
    public int TotalCount { get; set; }
    public List<VisitorDailyStat> DailyStats { get; set; } = new();
    public List<VisitorPageStat> TopPages { get; set; } = new();

    public VisitorStatsModel(IVisitorService visitorService)
    {
        _visitorService = visitorService;
    }

    public async Task OnGetAsync()
    {
        TodayCount = await _visitorService.GetTodayCountAsync();
        TotalCount = await _visitorService.GetTotalCountAsync();
        DailyStats = await _visitorService.GetDailyStatsAsync(30);
        TopPages = await _visitorService.GetTopPagesAsync(10);
    }
}
