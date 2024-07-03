using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
/// <param name="compositionStrategy"> The strategy that the composer should use to generate the composition. </param>
/// <param name="compositionDecorator"> The decorator that the composer should use to decorate the composition. </param>
/// <param name="compositionPhraser"> The phraser that the composer should use to phrase the composition. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose()
    {
        var composition = ComposeInitialComposition();

        return PhraseComposition(composition);
    }

    private Composition ComposeInitialComposition()
    {
        var measures = ComposeInitialMeasures();

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
        );

        while (measures.Count < compositionConfiguration.CompositionLength)
        {
            var initialChord = ComposeNextChord(compositionContext);
            var beats = new List<Beat> { new(initialChord) };

            compositionContext.Add(initialChord);

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var nextChord = ComposeNextChord(compositionContext);

                compositionContext.Add(nextChord);
                beats.Add(new Beat(nextChord));
            }

            measures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        var composition = new Composition(measures);

        compositionDecorator.Decorate(composition);

        return composition;
    }

    private Composition PhraseComposition(Composition initialComposition)
    {
        var currentMeasureIndex = 0;
        var phrasedMeasures = new List<Measure>();

        foreach (var measure in initialComposition.Measures)
        {
            phrasedMeasures.Add(measure);

            if (currentMeasureIndex++ % compositionConfiguration.PhrasingConfiguration.PhraseLengths.Min() == 0)
            {
                compositionPhraser.AttemptPhraseRepetition(phrasedMeasures);
            }

            if (phrasedMeasures.Count >= compositionConfiguration.CompositionLength)
            {
                break;
            }
        }

        var phrasedComposition = new Composition(phrasedMeasures);

        compositionDecorator.Decorate(phrasedComposition);
        compositionDecorator.ApplySustain(phrasedComposition);

        return phrasedComposition;
    }

    private List<Measure> ComposeInitialMeasures()
    {
        var initialChord = compositionStrategy.GenerateInitialChord();
        var beats = new List<Beat> { new(initialChord) };
        var precedingChords = beats.Select(beat => beat.Chord).ToList();

        while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
        {
            var nextChord = ComposeNextChord(precedingChords);

            precedingChords.Add(nextChord);
            beats.Add(new Beat(nextChord));
        }

        return [new Measure(beats, compositionConfiguration.Meter)];
    }

    private BaroquenChord ComposeNextChord(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(precedingChords);
        var chordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next()).First();

        return precedingChords[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);
    }
}
