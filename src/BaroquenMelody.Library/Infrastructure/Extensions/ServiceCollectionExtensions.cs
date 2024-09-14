using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Infrastructure.FileSystem;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

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
        .AddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>()
        .AddSingleton<IFile, FileWrapper>()
        .AddSingleton<IDirectory, DirectoryWrapper>()
        .AddSingleton<ICompositionConfigurationPersistenceService, CompositionConfigurationPersistenceService>()
        .AddSingleton<IBaroquenMelodyComposerConfigurator, BaroquenMelodyComposerConfigurator>()
        .AddSingleton<IOrnamentationConfigurationService, OrnamentationConfigurationService>()
        .AddSingleton<ICompositionRuleConfigurationService, CompositionRuleConfigurationService>()
        .AddSingleton<IInstrumentConfigurationService, InstrumentConfigurationService>()
        .AddSingleton<ICompositionConfigurationService, CompositionConfigurationService>();
}
