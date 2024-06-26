﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Infrastructure.Collections;

namespace BaroquenMelody.Library.Compositions.Ornamentation;

internal sealed class CompositionDecorator(IProcessor<OrnamentationItem> decorationEngine, CompositionConfiguration configuration) : ICompositionDecorator
{
    public void Decorate(Composition composition)
    {
        var beats = composition.Measures.SelectMany(measure => measure.Beats).ToList();
        var compositionContext = new FixedSizeList<Beat>(configuration.CompositionContextSize);
        var voices = configuration.VoiceConfigurations.Select(voiceConfiguration => voiceConfiguration.Voice);

        foreach (var voice in voices)
        {
            foreach (var currentBeat in beats)
            {
                var ornamentationItem = new OrnamentationItem(
                    voice,
                    compositionContext,
                    currentBeat,
                    beats.ElementAtOrDefault(beats.IndexOf(currentBeat) + 1)
                );

                decorationEngine.Process(ornamentationItem);

                compositionContext.Add(currentBeat);
            }
        }
    }
}
