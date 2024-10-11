using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal interface IOrnamentationEngineFactory
{
    IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration);
}
