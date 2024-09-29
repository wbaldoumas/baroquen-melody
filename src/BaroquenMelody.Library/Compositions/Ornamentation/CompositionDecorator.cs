using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation;

/// <inheritdoc cref="ICompositionDecorator"/>
internal sealed class CompositionDecorator(
    IProcessor<OrnamentationItem> ornamentationEngine,
    IProcessor<OrnamentationItem> sustainEngine,
    CompositionConfiguration configuration
) : ICompositionDecorator
{
    public void Decorate(Composition composition) => Decorate(composition, ornamentationEngine);

    public void ApplySustain(Composition composition) => Decorate(composition, sustainEngine);

    public void Decorate(Composition composition, Instrument instrument)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        Decorate(instrument, beats, compositionContext, ornamentationEngine);
    }

    private void Decorate(Composition composition, IProcessor<OrnamentationItem> processor)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var instruments = configuration.InstrumentConfigurations.Select(static instrumentConfiguration => instrumentConfiguration.Instrument);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        foreach (var instrument in instruments)
        {
            Decorate(instrument, beats, compositionContext, processor);
        }
    }

    private static void Decorate(Instrument instrument, List<Beat> beats, FixedSizeList<Beat> compositionContext, IProcessor<OrnamentationItem> processor)
    {
        for (var i = 0; i < beats.Count; ++i)
        {
            var currentBeat = beats[i];
            var nextBeat = beats.ElementAtOrDefault(i + 1);

            var ornamentationItem = new OrnamentationItem(
                instrument,
                compositionContext,
                currentBeat,
                nextBeat
            );

            processor.Process(ornamentationItem);

            compositionContext.Add(beats[i]);
        }
    }
}
