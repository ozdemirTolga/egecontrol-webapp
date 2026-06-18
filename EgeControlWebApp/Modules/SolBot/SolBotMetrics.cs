using System.Diagnostics.Metrics;

namespace EgeControlWebApp.Modules.SolBot;

public static class SolBotMetrics
{
    public static readonly Meter Meter = new("EgeControlWebApp.SolBot", "1.0.0");
    public static readonly Counter<int> FilterRejectCounter = Meter.CreateCounter<int>("solbot.filter.reject.count");
    public static readonly Counter<int> ExecutedTradesCounter = Meter.CreateCounter<int>("solbot.execution.count");
    public static readonly Histogram<double> SlippageHistogram = Meter.CreateHistogram<double>("solbot.execution.slippage_bps");
    public static readonly Histogram<double> RealizedPnlHistogram = Meter.CreateHistogram<double>("solbot.execution.realized_pnl_sol");
}
