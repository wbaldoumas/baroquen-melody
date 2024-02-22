﻿using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
/// <param name="compositionStrategy"> The strategy that the composer should use to generate the composition. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionStrategy compositionStrategy,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose()
    {
        var initialMeasure = ComposeInitialMeasure();

        var measures = new List<Measure>
        {
            initialMeasure
        };

        var currentChordContext = measures[0].Beats.Last().Chord.ChordContext;

        while (measures.Count < compositionConfiguration.CompositionLength)
        {
            var beats = new List<Beat>();

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var chordChoice = compositionStrategy.GetNextChordChoice(currentChordContext);
                var nextChord = currentChordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

                beats.Add(new Beat(nextChord));

                currentChordContext = nextChord.ChordContext;
            }

            measures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        return new Composition(measures);
    }

    private Measure ComposeInitialMeasure()
    {
        var initialChord = compositionStrategy.GetInitialChord();
        var chordContext = initialChord.ChordContext;

        var beats = new List<Beat> { new(initialChord) };

        while (beats.Count <= compositionConfiguration.Meter.BeatsPerMeasure())
        {
            var chordChoice = compositionStrategy.GetNextChordChoice(chordContext);
            var nextChord = initialChord.ChordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

            beats.Add(new Beat(nextChord));

            chordContext = nextChord.ChordContext;
        }

        return new Measure(beats, compositionConfiguration.Meter);
    }
}
