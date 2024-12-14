using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Output;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal sealed class OrnamentationProcessorFactory(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IOrnamentationProcessorConfigurationFactory configurationFactory,
    IOutputPolicy<OrnamentationItem> ornamentationCleaningOutputPolicy
) : IOrnamentationProcessorFactory
{
    public IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration) =>
        from configuration in compositionConfiguration.AggregateOrnamentationConfiguration.Configurations
        where configuration.IsEnabled
        from processorConfiguration in configurationFactory.Create(configuration)
        select PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(processorConfiguration.InputPolicies)
            .WithProcessors(new OrnamentationProcessor(musicalTimeSpanCalculator, compositionConfiguration, processorConfiguration))
            .WithOutputPolicies([.. processorConfiguration.OutputPolicies, ornamentationCleaningOutputPolicy])
            .Build();
}
