using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal sealed class OrnamentationProcessorFactory(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IOrnamentationProcessorConfigurationFactory configurationFactory
) : IOrnamentationProcessorFactory
{
    public IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration) =>
        from ornamentationConfiguration in compositionConfiguration.AggregateOrnamentationConfiguration.Configurations.Where(configuration => configuration.IsEnabled)
        from processorConfiguration in configurationFactory.Create(ornamentationConfiguration)
        select PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(processorConfiguration.InputPolicies)
            .WithProcessors(new OrnamentationProcessor(musicalTimeSpanCalculator, compositionConfiguration, processorConfiguration))
            .WithOutputPolicies(processorConfiguration.OutputPolicies)
            .Build();
}
