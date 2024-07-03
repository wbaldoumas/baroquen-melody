using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Infrastructure.Collections;

namespace BaroquenMelody.Library.Compositions.Ornamentation;

/// <inheritdoc cref="ICompositionDecorator"/>
internal sealed class CompositionDecorator(IProcessor<OrnamentationItem> decorationEngine, CompositionConfiguration configuration) : ICompositionDecorator
{
    public void Decorate(Composition composition)
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

                decorationEngine.Process(ornamentationItem);

                compositionContext.Add(beats[i]);
            }
        }
    }
}
