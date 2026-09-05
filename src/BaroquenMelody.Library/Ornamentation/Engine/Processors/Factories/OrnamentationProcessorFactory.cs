using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Output;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal sealed class OrnamentationProcessorFactory(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IOrnamentationProcessorConfigurationFactory configurationFactory,
    IOutputPolicy<OrnamentationItem> ornamentationLoggingOutputPolicy,
    IOutputPolicy<OrnamentationItem> ornamentationCleaningOutputPolicy,
    IVoiceRhythmPolicyTransformer voiceRhythmPolicyTransformer
) : IOrnamentationProcessorFactory
{
    // Processor order is load-bearing: it is the engine's unshuffled traversal order, and every processor draws
    // once per item it is offered, so it fixes the seeded draw sequence. Ordering by the enum keeps it a pure
    // function of the configuration rather than of whatever order the caller's set happens to enumerate in.
    // Output policies run in order too: the applied ornamentation is logged first, then the cross-voice cleaner
    // may strip it again.
    public IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration) =>
        from configuration in compositionConfiguration.AggregateOrnamentationConfiguration.Configurations
        where configuration.IsEnabled
        orderby configuration.OrnamentationType
        from processorConfiguration in configurationFactory.Create(configuration)
        select PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(voiceRhythmPolicyTransformer.Transform(configuration, processorConfiguration.InputPolicies))
            .WithProcessors(new OrnamentationProcessor(musicalTimeSpanCalculator, compositionConfiguration, processorConfiguration))
            .WithOutputPolicies(ornamentationLoggingOutputPolicy, ornamentationCleaningOutputPolicy)
            .Build();
}
