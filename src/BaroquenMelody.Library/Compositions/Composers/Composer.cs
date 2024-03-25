using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="BaroquenComposition"/>.
/// </summary>
/// <param name="compositionStrategy"> The strategy that the composer should use to generate the composition. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionStrategy compositionStrategy,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public BaroquenComposition Compose()
    {
        var measures = ComposeInitialMeasures();

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
        );

        while (measures.Count < compositionConfiguration.CompositionLength)
        {
            var initialChord = GenerateNextChord(compositionContext);
            var beats = new List<BaroquenBeat> { new(initialChord) };

            compositionContext.Add(initialChord);

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var nextChord = GenerateNextChord(compositionContext);

                compositionContext.Add(nextChord);
                beats.Add(new BaroquenBeat(nextChord));
            }

            measures.Add(new BaroquenMeasure(beats, compositionConfiguration.Meter));
        }

        return new BaroquenComposition(measures);
    }

    private List<BaroquenMeasure> ComposeInitialMeasures()
    {
        var initialChord = compositionStrategy.GenerateInitialChord();
        var beats = new List<BaroquenBeat> { new(initialChord) };
        var precedingChords = beats.Select(beat => beat.Chord).ToList();

        while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
        {
            var nextChord = GenerateNextChord(precedingChords);

            precedingChords.Add(nextChord);
            beats.Add(new BaroquenBeat(nextChord));
        }

        return [new BaroquenMeasure(beats, compositionConfiguration.Meter)];
    }

    private BaroquenChord GenerateNextChord(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(precedingChords);
        var chordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next(int.MaxValue)).First();

        return precedingChords[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);
    }
}
