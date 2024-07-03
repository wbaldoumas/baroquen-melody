using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
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

    private void Decorate(Composition composition, IProcessor<OrnamentationItem> processor)
    {
        var beats = composition.Measures.SelectMany(measure => measure.Beats).ToList();
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var voices = configuration.VoiceConfigurations.Select(voiceConfiguration => voiceConfiguration.Voice);

        foreach (var voice in voices)
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
}
