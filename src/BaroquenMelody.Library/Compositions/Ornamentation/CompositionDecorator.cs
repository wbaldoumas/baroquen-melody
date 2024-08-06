using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Infrastructure.Collections;

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

    public void Decorate(Composition composition, Voice voice)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        Decorate(voice, beats, compositionContext, ornamentationEngine);
    }

    private void Decorate(Composition composition, IProcessor<OrnamentationItem> processor)
    {
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var voices = configuration.VoiceConfigurations.Select(static voiceConfiguration => voiceConfiguration.Voice);
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        foreach (var voice in voices)
        {
            Decorate(voice, beats, compositionContext, processor);
        }
    }

    private static void Decorate(Voice voice, List<Beat> beats, FixedSizeList<Beat> compositionContext, IProcessor<OrnamentationItem> processor)
    {
        for (var i = 0; i < beats.Count; ++i)
        {
            var currentBeat = beats[i];
            var nextBeat = beats.ElementAtOrDefault(i + 1);

            var ornamentationItem = new OrnamentationItem(
                voice,
                compositionContext,
                currentBeat,
                nextBeat
            );

            processor.Process(ornamentationItem);

            compositionContext.Add(beats[i]);
        }
    }
}
