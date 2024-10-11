using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Utilities;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal sealed class OrnamentationEngineFactory(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IOrnamentationProcessorConfigurationFactory configurationFactory,
    ILogger logger
) : IOrnamentationEngineFactory
{
    public IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration) =>
        from ornamentationConfiguration in compositionConfiguration.AggregateOrnamentationConfiguration.Configurations.Where(configuration => configuration.IsEnabled)
        from processorConfiguration in configurationFactory.Create(ornamentationConfiguration)
        select PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(processorConfiguration.InputPolicies)
            .WithProcessors(new OrnamentationProcessor(musicalTimeSpanCalculator, compositionConfiguration, processorConfiguration))
            .WithOutputPolicies(new LogOrnamentation(processorConfiguration.OrnamentationType, logger))
            .Build();
}
