using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Contexts.Extensions;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
/// <param name="compositionStrategy"> The strategy that the composer should use to generate the composition. </param>
/// <param name="chordContextGenerator"> The generator to use to generate chord contexts. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionStrategy compositionStrategy,
    IChordContextGenerator chordContextGenerator,
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

        var currentChordContext = measures[^1].Beats.Last().Chord.ArrivedFromChordContext;

        while (measures.Count < compositionConfiguration.CompositionLength)
        {
            var beats = new List<Beat>();

            var previousChord = measures[^1].Beats.Last().Chord;

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var chordChoice = compositionStrategy.GetNextChordChoice(currentChordContext);
                var nextChord = currentChordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

                beats.Add(new Beat(nextChord));

                currentChordContext = chordContextGenerator.GenerateChordContext(previousChord, nextChord);
                previousChord = nextChord;
            }

            measures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        return new Composition(measures);
    }

    private Measure ComposeInitialMeasure()
    {
        var initialChord = compositionStrategy.GetInitialChord();
        var chordContext = initialChord.ArrivedFromChordContext;

        var beats = new List<Beat> { new(initialChord) };

        while (beats.Count <= compositionConfiguration.Meter.BeatsPerMeasure() - 1)
        {
            var chordChoice = compositionStrategy.GetNextChordChoice(chordContext);
            var nextChord = initialChord.ArrivedFromChordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

            beats.Add(new Beat(nextChord));

            chordContext = nextChord.ArrivedFromChordContext;
        }

        return new Measure(beats, compositionConfiguration.Meter);
    }
}
