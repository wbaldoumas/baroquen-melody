using BaroquenMelody.Library.Compositions.Configurations.Services;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Infrastructure.Extensions;

[ExcludeFromCodeCoverage(Justification = "Simple container configuration")]
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
        .AddSingleton<IInstrumentConfigurationService, InstrumentConfigurationService>()
        .AddSingleton<ICompositionConfigurationService, CompositionConfigurationService>();
}
