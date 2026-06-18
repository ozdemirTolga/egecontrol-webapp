using System.Text.Json.Serialization;

namespace EgeControlWebApp.Modules.SolBot;

public enum TradingMode
{
    Paper = 0,
    Live = 1
}

public enum DexKind
{
    Unknown = 0,
    RaydiumV4 = 1,
    Meteora = 2,
    PumpFun = 3
}

public enum SolBotEventType
{
    Unknown = 0,
    PoolCreated = 1,
    Swap = 2,
    LiquidityAdded = 3
}

public enum TradeSide
{
    Buy = 0,
    Sell = 1
}

public enum FilterStatus
{
    Allow = 0,
    Quarantine = 1,
    Reject = 2
}

public enum RiskTriggerType
{
    Hold = 0,
    TakeProfit = 1,
    StopLoss = 2,
    TrailingStop = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WalletTier
{
    Basic = 0,
    Advanced = 1,
    Pro = 2,
    Elite = 3
}

public sealed record NormalizedSolanaEvent(
    string EventId,
    string Source,
    long Slot,
    DateTimeOffset ObservedAt,
    DexKind Dex,
    SolBotEventType EventType,
    string TokenMint,
    string PoolAddress,
    decimal LiquiditySol,
    string RawPayloadJson);

public sealed record TokenSecuritySnapshot(
    string TokenMint,
    decimal RugCheckScore,
    bool MintAuthorityEnabled,
    bool FreezeAuthorityEnabled,
    decimal LiquiditySol,
    decimal TopHolderPercent,
    decimal TopTenHolderPercent,
    int UniqueHolders,
    decimal WashTradingScore);

public sealed record FilterDecision(FilterStatus Status, decimal SafetyScore, IReadOnlyList<string> Reasons)
{
    public bool IsAllowed => Status == FilterStatus.Allow;
}

public sealed record TakeProfitLevel(decimal GainPercent, decimal ExitFraction);

public sealed record PositionRiskProfile(decimal StopLossPercent, decimal TrailingStopPercent, IReadOnlyList<TakeProfitLevel> TakeProfits);

public sealed record StrategySignal(
    string TokenMint,
    string PoolAddress,
    DexKind Dex,
    decimal AllocationSol,
    bool FirstBlockOpportunity,
    PositionRiskProfile RiskProfile,
    string? WalletAddress = null);

public sealed record PositionState(
    Guid PositionId,
    string TokenMint,
    decimal QuantityTokens,
    decimal AverageEntryPriceSol,
    decimal HighestPriceSeenSol,
    decimal RemainingQuantityTokens,
    IReadOnlySet<int> TriggeredTakeProfitIndexes);

public sealed record RiskEvaluation(RiskTriggerType Trigger, decimal QuantityToExitTokens, decimal UpdatedHighWaterMarkSol, IReadOnlyList<string> Reasons)
{
    public bool ShouldExit => Trigger != RiskTriggerType.Hold && QuantityToExitTokens > 0;
}

public sealed record MarketDepthSnapshot(
    decimal BaseReserveSol,
    decimal QuoteReserveTokens,
    decimal PoolFeeBps,
    decimal BuyTaxBps,
    decimal SellTaxBps,
    decimal LatencyBps);

public sealed record ExecutionRequest(
    TradingMode Mode,
    TradeSide Side,
    string TokenMint,
    string PoolAddress,
    DexKind Dex,
    decimal InputAmount,
    MarketDepthSnapshot MarketDepth,
    PositionRiskProfile RiskProfile,
    decimal WalletSolBotBalance,
    decimal AvailableCashSol,
    decimal ExpectedEdgeSol,
    PriorityFeeSnapshot PriorityFeeSnapshot,
    bool FirstBlockOpportunity,
    string? WalletAddress = null,
    string? EncryptedPrivateKey = null,
    string? UnsignedTransaction = null,
    string? CorrelationId = null);

public sealed record PricingSimulationResult(
    decimal InputAmount,
    decimal OutputAmount,
    decimal ExecutionPriceSol,
    decimal SlippageBps,
    decimal PoolFeeSol,
    decimal TokenTaxSol,
    decimal NetworkFeeSol,
    decimal JitoTipSol,
    decimal CommissionSol);

public sealed record PriorityFeeSnapshot(decimal Low, decimal Medium, decimal High, decimal VeryHigh);

public sealed record PriorityFeeQuote(decimal MicroLamports, int ComputeUnitLimit);

public sealed record BundleSubmissionRequest(string UnsignedTransaction, long JitoTipLamports, decimal PriorityFeeMicroLamports, int ComputeUnitLimit);

public sealed record BundleSubmissionReceipt(string BundleId, bool Submitted, string Endpoint, string PayloadJson);

public sealed record ExecutionResult(
    bool Success,
    TradeSide Side,
    string TokenMint,
    decimal InputAmount,
    decimal OutputAmount,
    decimal ExecutionPriceSol,
    decimal FeesSol,
    decimal RealizedPnlSol,
    Guid? PositionId,
    string Mode,
    string Details,
    string? ExternalReference = null);

public sealed record RugCheckAssessment(string TokenMint, decimal Score, IReadOnlyList<string> Warnings);

public sealed record RevenueShareAccrual(string WalletAddress, decimal GrossCommissionSol, decimal DiscountedCommissionSol, decimal RevenueShareAccruedSol, WalletTier Tier);

public interface IMarketEventNormalizer
{
    NormalizedSolanaEvent Normalize(string transport, string payloadJson);
}

public interface ITokenFilterEngine
{
    FilterDecision Evaluate(TokenSecuritySnapshot snapshot);
}

public interface ISniperStrategyEngine
{
    StrategySignal? TryCreateSignal(NormalizedSolanaEvent marketEvent, FilterDecision filterDecision, decimal availableCashSol, int openPositionCount);
}

public interface IRiskEngine
{
    RiskEvaluation Evaluate(PositionState position, PositionRiskProfile profile, decimal currentPriceSol);
}

public interface ISlippagePricingEngine
{
    PricingSimulationResult Simulate(ExecutionRequest request, decimal priorityFeeMicroLamports, long jitoTipLamports, decimal commissionBps);
}

public interface IPriorityFeeEstimator
{
    PriorityFeeQuote Estimate(PriorityFeeSnapshot snapshot, bool urgent);
}

public interface IJitoTipCalculator
{
    long CalculateLamports(decimal expectedEdgeSol, double congestionLevel, bool urgent);
}

public interface IWalletTierService
{
    WalletTier ResolveTier(decimal walletBalance);
    decimal ResolveCommissionDiscountBps(decimal walletBalance);
}

public interface IRevenueShareService
{
    RevenueShareAccrual CreateAccrual(string walletAddress, decimal commissionSol, decimal walletBalance);
}

public interface IExecutionAdapter
{
    Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default);
}

public interface IEncryptedPrivateKeyStore
{
    string Protect(string privateKey);
    string Unprotect(string encryptedPrivateKey);
}

public interface IRugCheckClient
{
    Task<RugCheckAssessment> GetAssessmentAsync(string tokenMint, CancellationToken cancellationToken = default);
}

public interface IJitoBundleSubmitter
{
    Task<BundleSubmissionReceipt> SubmitAsync(BundleSubmissionRequest request, CancellationToken cancellationToken = default);
}
