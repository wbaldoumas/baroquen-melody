using Atrea.PolicyEngine;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rhythm;

namespace BaroquenMelody.Library.Ornamentation;

/// <inheritdoc cref="ICompositionDecorator"/>
/// <remarks>
///     Under an active accompaniment texture, the whole-composition ornamentation pass decorates in the
///     scheduler's register order - melody first, figuration last - so within each pass the cleaners'
///     just-decorated-loses rule resolves a dissonant coincidence in the melody's favor (its callers: the
///     fugal composer's two body passes and the ending composer's bridging and tonic sub-compositions,
///     which hold no recorded notes and so re-order harmlessly). Only that one method re-orders: the
///     sustain pass keeps the raw configuration order, because its gate draws once per item and
///     re-ordering it would re-order the draws on the shared stream, and the per-instrument overload is
///     unaffected - it takes an explicit instrument and has no order at all.
/// </remarks>
internal sealed class CompositionDecorator(
    IPolicyEngine<OrnamentationItem> ornamentationEngine,
    IPolicyEngine<OrnamentationItem> sustainEngine,
    IVoiceRhythmScheduler voiceRhythmScheduler,
    CompositionConfiguration configuration
) : ICompositionDecorator
{
    public void Decorate(Composition composition) => Decorate(composition, ornamentationEngine, ResolveOrnamentationInstrumentOrder(), shuffleProcessors: configuration.ShuffleOrnamentationProcessors);

    public void ApplySustain(Composition composition) => Decorate(composition, sustainEngine, GetConfiguredInstrumentOrder());

    public void Decorate(Composition composition, Instrument instrument)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        Decorate(instrument, beats, compositionContext, ornamentationEngine, shuffleProcessors: configuration.ShuffleOrnamentationProcessors);
    }

    private void Decorate(Composition composition, IPolicyEngine<OrnamentationItem> processor, IEnumerable<Instrument> instruments, bool shuffleProcessors = false)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        foreach (var instrument in instruments)
        {
            Decorate(instrument, beats, compositionContext, processor, shuffleProcessors);
        }
    }

    private IEnumerable<Instrument> ResolveOrnamentationInstrumentOrder() =>
        voiceRhythmScheduler.TryGetTextureDecorationOrder(out var textureDecorationOrder)
            ? textureDecorationOrder
            : GetConfiguredInstrumentOrder();

    private IEnumerable<Instrument> GetConfiguredInstrumentOrder() =>
        configuration.InstrumentConfigurations.Select(static instrumentConfiguration => instrumentConfiguration.Instrument);

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
