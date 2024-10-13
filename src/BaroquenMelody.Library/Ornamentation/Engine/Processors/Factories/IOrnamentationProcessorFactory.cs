using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal interface IOrnamentationProcessorFactory
{
    IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration);
}
