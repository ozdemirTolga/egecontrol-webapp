using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EgeControlWebApp.Modules.SolBot.Persistence;

public sealed class SolBotTradeRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(64)]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    [MaxLength(16)]
    public string Mode { get; set; } = string.Empty;
    [MaxLength(8)]
    public string Side { get; set; } = string.Empty;
    [MaxLength(128)]
    public string TokenMint { get; set; } = string.Empty;
    [MaxLength(128)]
    public string PoolAddress { get; set; } = string.Empty;
    [MaxLength(32)]
    public string Dex { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,9)")]
    public decimal InputAmount { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal OutputAmount { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal ExecutionPriceSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal FeesSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal RealizedPnlSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal RevenueShareAccrualSol { get; set; }
    [MaxLength(32)]
    public string Status { get; set; } = "Filled";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? PositionId { get; set; }
    public SolBotPositionRecord? Position { get; set; }
    public ICollection<SolBotFillRecord> Fills { get; set; } = new List<SolBotFillRecord>();
}

public sealed class SolBotFillRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TradeId { get; set; }
    public SolBotTradeRecord Trade { get; set; } = default!;
    [MaxLength(16)]
    public string FillType { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,9)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal PriceSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal SlippageBps { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal PoolFeeSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal NetworkFeeSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal JitoTipSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal CommissionSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal TokenTaxSol { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SolBotPositionRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(128)]
    public string TokenMint { get; set; } = string.Empty;
    [MaxLength(128)]
    public string PoolAddress { get; set; } = string.Empty;
    [MaxLength(32)]
    public string Dex { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,9)")]
    public decimal QuantityTokens { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal RemainingQuantityTokens { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal AverageEntryPriceSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal HighestObservedPriceSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal StopLossPercent { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal TrailingStopPercent { get; set; }
    public string TakeProfitPlanJson { get; set; } = "[]";
    public string TriggeredTakeProfitsJson { get; set; } = "[]";
    [MaxLength(64)]
    public string? LastExitTrigger { get; set; }
    [MaxLength(32)]
    public string Status { get; set; } = "Open";
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
    public ICollection<SolBotTradeRecord> Trades { get; set; } = new List<SolBotTradeRecord>();
}

public sealed class SolBotBalanceSnapshotRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(128)]
    public string TokenMint { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,9)")]
    public decimal CashBalanceSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal UnrealizedPnlSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal RealizedPnlSol { get; set; }
    [Column(TypeName = "decimal(20,9)")]
    public decimal EquitySol { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SolBotMetricRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;
    [Column(TypeName = "decimal(20,9)")]
    public decimal Value { get; set; }
    public string TagsJson { get; set; } = "{}";
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SolBotEventLogRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(128)]
    public string EventType { get; set; } = string.Empty;
    [MaxLength(32)]
    public string Source { get; set; } = string.Empty;
    public long Slot { get; set; }
    [MaxLength(128)]
    public string TokenMint { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
