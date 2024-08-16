using BaroquenMelody.Library.Compositions.Configurations.Services;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;

namespace BaroquenMelody.Library.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaroquenMelody(this IServiceCollection services) => services
        .AddFluxor(fluxorOptions =>
        {
            fluxorOptions.WithLifetime(StoreLifetime.Singleton);
            fluxorOptions.ScanAssemblies(typeof(BaroquenMelodyComposerConfigurator).Assembly);
        })
        .AddSingleton<IBaroquenMelodyComposerConfigurator, BaroquenMelodyComposerConfigurator>()
        .AddSingleton<IOrnamentationConfigurationService, OrnamentationConfigurationService>()
        .AddSingleton<ICompositionRuleConfigurationService, CompositionRuleConfigurationService>()
        .AddSingleton<IVoiceConfigurationService, VoiceConfigurationService>()
        .AddSingleton<ICompositionConfigurationService, CompositionConfigurationService>();
}
