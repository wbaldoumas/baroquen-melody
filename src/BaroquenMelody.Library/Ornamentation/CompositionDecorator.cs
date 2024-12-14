using Atrea.PolicyEngine;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Ornamentation;

/// <inheritdoc cref="ICompositionDecorator"/>
internal sealed class CompositionDecorator(
    IPolicyEngine<OrnamentationItem> ornamentationEngine,
    IPolicyEngine<OrnamentationItem> sustainEngine,
    CompositionConfiguration configuration
) : ICompositionDecorator
{
    public void Decorate(Composition composition) => Decorate(composition, ornamentationEngine, shuffleProcessors: true);

    public void ApplySustain(Composition composition) => Decorate(composition, sustainEngine);

    public void Decorate(Composition composition, Instrument instrument)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        Decorate(instrument, beats, compositionContext, ornamentationEngine, shuffleProcessors: true);
    }

    private void Decorate(Composition composition, IPolicyEngine<OrnamentationItem> processor, bool shuffleProcessors = false)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var instruments = configuration.InstrumentConfigurations.Select(static instrumentConfiguration => instrumentConfiguration.Instrument);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        foreach (var instrument in instruments)
        {
            Decorate(instrument, beats, compositionContext, processor, shuffleProcessors);
        }
    }

    private static void Decorate(
        Instrument instrument,
        List<Beat> beats,
        FixedSizeList<Beat> compositionContext,
        IPolicyEngine<OrnamentationItem> processor,
        bool shuffleProcessors = false)
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

            if (shuffleProcessors)
            {
                processor.Shuffle();
            }

            compositionContext.Add(beats[i]);
        }
    }
}
