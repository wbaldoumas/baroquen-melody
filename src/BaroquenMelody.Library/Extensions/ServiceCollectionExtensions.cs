using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Midi.Repositories;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Providers;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Rules;
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
        .AddSingleton<IMidiExampleGenerator, MidiExampleGenerator>()
        .AddSingleton<IWeightedRandomBooleanGenerator, WeightedRandomBooleanGenerator>()
        .AddSingleton<IVoiceSpacingSatisfiabilityAnalyzer, VoiceSpacingSatisfiabilityAnalyzer>()
        .AddSingleton<IGroundBassFeasibilityAnalyzer, GroundBassFeasibilityAnalyzer>()

        // Both of the configurator's random providers (the composition stream and the processor-shuffle stream)
        // resolve to this one unseeded singleton: the streams only need to be distinct under a seed, which is a
        // test-side construction (SeededRandomProviders), never a registration. A future production seeded mode must
        // register two DISTINCT providers (e.g. keyed services), or the two streams silently alias.
        .AddSingleton<IRandomProvider, ThreadLocalRandomProvider>()
        .AddSingleton<IMusicalTimeSpanCalculator, MusicalTimeSpanCalculator>()
        .AddSingleton<IOrnamentationProcessorConfigurationFactoryProvider, OrnamentationProcessorConfigurationFactoryProvider>();
}
