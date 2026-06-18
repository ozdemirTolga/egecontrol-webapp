using Microsoft.Extensions.Options;

namespace EgeControlWebApp.Modules.SolBot;

public static class SolBotServiceCollectionExtensions
{
    public static IServiceCollection AddSolBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SolBotOptions>()
            .Bind(configuration.GetSection(SolBotOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddSingleton<IMarketEventNormalizer, SolanaEventNormalizer>();
        services.AddScoped<ITokenFilterEngine, TokenFilterEngine>();
        services.AddSingleton<ISniperStrategyEngine, SniperStrategyEngine>();
        services.AddSingleton<IRiskEngine, RiskEngine>();
        services.AddSingleton<IWalletTierService, WalletTierService>();
        services.AddSingleton<IRevenueShareService, RevenueShareService>();
        services.AddSingleton<IPriorityFeeEstimator, HeliusPriorityFeeEstimator>();
        services.AddSingleton<IJitoTipCalculator, JitoTipCalculator>();
        services.AddScoped<ISlippagePricingEngine, SlippagePricingEngine>();
        services.AddSingleton<IEncryptedPrivateKeyStore, EncryptedPrivateKeyStore>();
        services.AddScoped<PaperExecutionAdapter>();
        services.AddScoped<LiveExecutionAdapter>();
        services.AddHttpClient<IRugCheckClient, RugCheckClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SolBotOptions>>().Value;
            client.BaseAddress = new Uri(options.Endpoints.RugCheckBaseUrl);
        });
        services.AddHttpClient<IJitoBundleSubmitter, HttpJitoBundleSubmitter>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SolBotOptions>>().Value;
            client.BaseAddress = new Uri(options.Endpoints.JitoBlockEngineUrl);
        });

        return services;
    }
}
