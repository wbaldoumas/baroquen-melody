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
        var measures = new List<Measure>();

        while (measures.Count < compositionConfiguration.CompositionLength)
        {
            var beats = new List<Beat>();

            ContextualizedChord? previousChord;

            if (measures.Count == 0)
            {
                beats.Add(new Beat(compositionStrategy.GetInitialChord()));
                previousChord = beats[^1].Chord;
            }
            else
            {
                previousChord = measures[^1].Beats[^1].Chord;
            }

            var previousChordContext = previousChord.ArrivedFromChordContext;

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var chordChoice = compositionStrategy.GetNextChordChoice(previousChordContext);
                var nextChord = previousChord.ArrivedFromChordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

                beats.Add(new Beat(nextChord));

                previousChordContext = chordContextGenerator.GenerateChordContext(previousChord, nextChord);
                previousChord = nextChord;
            }

            measures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        return new Composition(measures);
    }
}
