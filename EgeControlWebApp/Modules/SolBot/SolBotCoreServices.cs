using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EgeControlWebApp.Modules.SolBot;

public sealed class SolanaEventNormalizer : IMarketEventNormalizer
{
    private static readonly IReadOnlyDictionary<string, DexKind> ProgramMappings = new Dictionary<string, DexKind>(StringComparer.OrdinalIgnoreCase)
    {
        ["raydium"] = DexKind.RaydiumV4,
        ["raydium-v4"] = DexKind.RaydiumV4,
        ["675kPX9MHTjS2zt1qfr1NYHuzeLXfQM9H24wFSUt1Mp8"] = DexKind.RaydiumV4,
        ["meteora"] = DexKind.Meteora,
        ["LBUZKhRxPF3XUpBCS9YLciF4GfLxLxWM4xW8b4Yh2kM"] = DexKind.Meteora,
        ["pumpfun"] = DexKind.PumpFun,
        ["6EF8rrecthR5Dkzon8Nwu78hRvfCKubJ14M5uBEwF6P"] = DexKind.PumpFun
    };

    public NormalizedSolanaEvent Normalize(string transport, string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;
        var transportKey = transport.Trim().ToLowerInvariant();

        return transportKey switch
        {
            "helius" or "helius-ws" or "quicknode" => NormalizeFlatPayload(transportKey, root, payloadJson),
            "yellowstone" or "geyser" or "yellowstone-grpc" => NormalizeGeyserPayload(transportKey, root, payloadJson),
            _ => throw new InvalidOperationException($"Unsupported SolBot transport '{transport}'.")
        };
    }

    private static NormalizedSolanaEvent NormalizeFlatPayload(string transport, JsonElement root, string rawPayloadJson)
    {
        var source = GetString(root, "source", transport);
        var slot = GetInt64(root, "slot");
        var dex = ResolveDex(GetString(root, "dex", GetString(root, "programId", "unknown")));
        var eventType = ResolveEventType(GetString(root, "eventType", "pool_create"));
        var tokenMint = GetString(root, "tokenMint");
        var poolAddress = GetString(root, "poolAddress");
        var liquidity = GetDecimal(root, "liquiditySol");
        var eventId = GetString(root, "eventId", $"{source}:{slot}:{poolAddress}");
        var observedAt = GetDateTime(root, "observedAt", DateTimeOffset.UtcNow);

        return new NormalizedSolanaEvent(eventId, source, slot, observedAt, dex, eventType, tokenMint, poolAddress, liquidity, rawPayloadJson);
    }

    private static NormalizedSolanaEvent NormalizeGeyserPayload(string transport, JsonElement root, string rawPayloadJson)
    {
        var instruction = root.GetProperty("instruction");
        var accounts = instruction.GetProperty("accounts");
        var source = GetString(root, "source", transport);
        var slot = GetInt64(root, "slot");
        var dex = ResolveDex(GetString(instruction, "programId", "unknown"));
        var eventType = ResolveEventType(GetString(instruction, "eventType", "pool_create"));
        var tokenMint = GetString(accounts, "tokenMint");
        var poolAddress = GetString(accounts, "poolAddress");
        var liquidity = GetDecimal(accounts, "liquiditySol");
        var signature = GetString(root, "signature", $"{source}-{slot}");
        var eventId = $"{signature}:{slot}:{poolAddress}";
        var observedAt = GetDateTime(root, "observedAt", DateTimeOffset.UtcNow);

        return new NormalizedSolanaEvent(eventId, source, slot, observedAt, dex, eventType, tokenMint, poolAddress, liquidity, rawPayloadJson);
    }

    private static DexKind ResolveDex(string value)
        => ProgramMappings.TryGetValue(value, out var dex) ? dex : DexKind.Unknown;

    private static SolBotEventType ResolveEventType(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "pool_create" or "poolcreated" => SolBotEventType.PoolCreated,
            "swap" => SolBotEventType.Swap,
            "liquidity_add" or "liquidityadded" => SolBotEventType.LiquidityAdded,
            _ => SolBotEventType.Unknown
        };

    private static string GetString(JsonElement element, string propertyName, string? fallback = null)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString()!;
        }

        if (fallback is not null)
        {
            return fallback;
        }

        throw new InvalidOperationException($"Missing string property '{propertyName}'.");
    }

    private static long GetInt64(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.TryGetInt64(out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"Missing integer property '{propertyName}'.");
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.String when decimal.TryParse(property.GetString(), CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => throw new InvalidOperationException($"Invalid numeric property '{propertyName}'.")
        };
    }

    private static DateTimeOffset GetDateTime(JsonElement element, string propertyName, DateTimeOffset fallback)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(property.GetString(), out var parsed))
        {
            return parsed;
        }

        return fallback;
    }
}

public sealed class TokenFilterEngine : ITokenFilterEngine
{
    private readonly SolBotFilterOptions _options;
    private readonly ILogger<TokenFilterEngine> _logger;

    public TokenFilterEngine(IOptions<SolBotOptions> options, ILogger<TokenFilterEngine> logger)
    {
        _options = options.Value.Filters;
        _logger = logger;
    }

    public FilterDecision Evaluate(TokenSecuritySnapshot snapshot)
    {
        var reasons = new List<string>();
        decimal safetyScore = snapshot.RugCheckScore;

        if (_options.RequireMintAuthorityRevoked && snapshot.MintAuthorityEnabled)
        {
            reasons.Add("mint_authority_enabled");
            safetyScore -= 20m;
        }

        if (_options.RequireFreezeAuthorityRevoked && snapshot.FreezeAuthorityEnabled)
        {
            reasons.Add("freeze_authority_enabled");
            safetyScore -= 20m;
        }

        if (snapshot.LiquiditySol < _options.MinLiquiditySol)
        {
            reasons.Add("insufficient_liquidity");
            safetyScore -= 15m;
        }

        if (snapshot.TopHolderPercent > _options.MaxTopHolderPercent)
        {
            reasons.Add("top_holder_concentration");
            safetyScore -= 15m;
        }

        if (snapshot.TopTenHolderPercent > _options.MaxTopTenHoldersPercent)
        {
            reasons.Add("top_10_holder_concentration");
            safetyScore -= 10m;
        }

        if (snapshot.UniqueHolders < _options.MinUniqueHolders)
        {
            reasons.Add("not_enough_unique_holders");
            safetyScore -= 10m;
        }

        if (snapshot.WashTradingScore > _options.MaxWashTradingScore)
        {
            reasons.Add("wash_trading_detected");
            safetyScore -= 20m;
        }

        var status = reasons.Count switch
        {
            0 when safetyScore >= _options.MinSafetyScore => FilterStatus.Allow,
            _ when safetyScore >= _options.MinSafetyScore - _options.QuarantineScoreGrace && reasons.All(reason => reason is "not_enough_unique_holders") => FilterStatus.Quarantine,
            _ => FilterStatus.Reject
        };

        if (status == FilterStatus.Reject)
        {
            SolBotMetrics.FilterRejectCounter.Add(1);
            _logger.LogInformation("SolBot rejected {TokenMint} with score {SafetyScore} because {Reasons}", snapshot.TokenMint, safetyScore, reasons);
        }

        return new FilterDecision(status, Math.Max(0m, safetyScore), reasons);
    }
}

public sealed class SniperStrategyEngine : ISniperStrategyEngine
{
    private readonly SolBotOptions _options;

    public SniperStrategyEngine(IOptions<SolBotOptions> options)
    {
        _options = options.Value;
    }

    public StrategySignal? TryCreateSignal(NormalizedSolanaEvent marketEvent, FilterDecision filterDecision, decimal availableCashSol, int openPositionCount)
    {
        if (!_options.Enabled || !filterDecision.IsAllowed)
        {
            return null;
        }

        if (openPositionCount >= _options.Risk.MaxConcurrentPositions)
        {
            return null;
        }

        if (_options.Strategy.FirstBlockOnly && marketEvent.EventType != SolBotEventType.PoolCreated)
        {
            return null;
        }

        var allocation = Math.Min(availableCashSol * _options.Strategy.AllocationFraction, _options.Strategy.MaxAllocationSol);
        allocation = Math.Max(allocation, _options.Strategy.MinAllocationSol);

        if (allocation > availableCashSol)
        {
            return null;
        }

        var takeProfits = _options.Risk.TakeProfits
            .Select(level => new TakeProfitLevel(level.GainPercent, level.ExitFraction))
            .ToArray();

        return new StrategySignal(
            marketEvent.TokenMint,
            marketEvent.PoolAddress,
            marketEvent.Dex,
            allocation,
            marketEvent.EventType == SolBotEventType.PoolCreated,
            new PositionRiskProfile(_options.Risk.StopLossPercent, _options.Risk.TrailingStopPercent, takeProfits));
    }
}

public sealed class RiskEngine : IRiskEngine
{
    public RiskEvaluation Evaluate(PositionState position, PositionRiskProfile profile, decimal currentPriceSol)
    {
        var reasons = new List<string>();
        var highWaterMark = Math.Max(position.HighestPriceSeenSol, currentPriceSol);
        var stopLossPrice = position.AverageEntryPriceSol * (1 - profile.StopLossPercent);
        var trailingStopPrice = highWaterMark * (1 - profile.TrailingStopPercent);

        if (currentPriceSol <= stopLossPrice)
        {
            reasons.Add("stop_loss_hit");
            return new RiskEvaluation(RiskTriggerType.StopLoss, position.RemainingQuantityTokens, highWaterMark, reasons);
        }

        if (currentPriceSol <= trailingStopPrice && highWaterMark > position.AverageEntryPriceSol)
        {
            reasons.Add("trailing_stop_hit");
            return new RiskEvaluation(RiskTriggerType.TrailingStop, position.RemainingQuantityTokens, highWaterMark, reasons);
        }

        for (var index = 0; index < profile.TakeProfits.Count; index++)
        {
            if (position.TriggeredTakeProfitIndexes.Contains(index))
            {
                continue;
            }

            var takeProfit = profile.TakeProfits[index];
            var targetPrice = position.AverageEntryPriceSol * (1 + takeProfit.GainPercent);
            if (currentPriceSol >= targetPrice)
            {
                reasons.Add($"take_profit_{index + 1}_hit");
                var quantity = Math.Min(position.RemainingQuantityTokens, position.QuantityTokens * takeProfit.ExitFraction);
                return new RiskEvaluation(RiskTriggerType.TakeProfit, quantity, highWaterMark, reasons);
            }
        }

        return new RiskEvaluation(RiskTriggerType.Hold, 0m, highWaterMark, reasons);
    }
}

public sealed class WalletTierService : IWalletTierService
{
    private readonly IReadOnlyList<SolBotTierOptions> _tiers;

    public WalletTierService(IOptions<SolBotOptions> options)
    {
        _tiers = options.Value.Token.Tiers.OrderBy(tier => tier.MinimumBalance).ToArray();
    }

    public WalletTier ResolveTier(decimal walletBalance)
        => _tiers.LastOrDefault(tier => walletBalance >= tier.MinimumBalance)?.Tier ?? WalletTier.Basic;

    public decimal ResolveCommissionDiscountBps(decimal walletBalance)
        => _tiers.LastOrDefault(tier => walletBalance >= tier.MinimumBalance)?.CommissionDiscountBps ?? 0m;
}

public sealed class RevenueShareService : IRevenueShareService
{
    private readonly IWalletTierService _walletTierService;

    public RevenueShareService(IWalletTierService walletTierService)
    {
        _walletTierService = walletTierService;
    }

    public RevenueShareAccrual CreateAccrual(string walletAddress, decimal commissionSol, decimal walletBalance)
    {
        var tier = _walletTierService.ResolveTier(walletBalance);
        var discountedCommission = commissionSol * (1 - (_walletTierService.ResolveCommissionDiscountBps(walletBalance) / 10_000m));
        var revenueShare = discountedCommission * (tier switch
        {
            WalletTier.Basic => 0m,
            WalletTier.Advanced => 0.05m,
            WalletTier.Pro => 0.1m,
            WalletTier.Elite => 0.15m,
            _ => 0m
        });

        return new RevenueShareAccrual(walletAddress, commissionSol, discountedCommission, revenueShare, tier);
    }
}
