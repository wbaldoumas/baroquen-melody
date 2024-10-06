using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Midi.Repositories;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace BaroquenMelody.Library.Extensions;

[ExcludeFromCodeCoverage(Justification = "Simple container configuration")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaroquenMelody(this IServiceCollection services) => services
        .AddFluxor(fluxorOptions =>
        {
            fluxorOptions.WithLifetime(StoreLifetime.Singleton);
            fluxorOptions.ScanAssemblies(typeof(BaroquenMelodyComposerConfigurator).Assembly);
        })
        .AddSingleton<IFileSystem, FileSystem>()
        .AddSingleton<IFile, FileWrapper>()
        .AddSingleton<IDirectory, DirectoryWrapper>()
        .AddSingleton<ICompositionConfigurationPersistenceService, CompositionConfigurationPersistenceService>()
        .AddSingleton<IBaroquenMelodyComposerConfigurator, BaroquenMelodyComposerConfigurator>()
        .AddSingleton<IOrnamentationConfigurationService, OrnamentationConfigurationService>()
        .AddSingleton<ICompositionRuleConfigurationService, CompositionRuleConfigurationService>()
        .AddSingleton<IInstrumentConfigurationService, InstrumentConfigurationService>()
        .AddSingleton<ICompositionConfigurationService, CompositionConfigurationService>()
        .AddSingleton<IMidiInstrumentRepository, MidiInstrumentRepository>()
        .AddSingleton<IMidiExampleGenerator, MidiExampleGenerator>();
}
