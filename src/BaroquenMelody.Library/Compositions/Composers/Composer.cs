using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Infrastructure.Collections;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
/// <param name="compositionDecorator"> The decorator that the composer should use to decorate the composition. </param>
/// <param name="compositionPhraser"> The phraser that the composer should use to phrase the composition. </param>
/// <param name="chordComposer"> The chord composer that the composer should use to compose chords. </param>
/// <param name="themeComposer"> The theme composer that the composer should use to compose themes. </param>
/// <param name="endingComposer"> The ending composer that the composer should use to compose endings. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    IChordComposer chordComposer,
    IThemeComposer themeComposer,
    IEndingComposer endingComposer,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose()
    {
        var theme = themeComposer.Compose();

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            theme.Exposition.SelectMany(measure => measure.Beats.Select(beat => new BaroquenChord(beat.Chord)))
        );

        var continuationMeasures = new List<Measure>();

        while (continuationMeasures.Count < compositionConfiguration.CompositionLength)
        {
            var initialChord = chordComposer.Compose(compositionContext);
            var beats = new List<Beat>(compositionConfiguration.BeatsPerMeasure) { new(initialChord) };

            compositionContext.Add(initialChord);

            while (beats.Count < compositionConfiguration.BeatsPerMeasure)
            {
                var nextChord = chordComposer.Compose(compositionContext);

                compositionContext.Add(nextChord);
                beats.Add(new Beat(nextChord));
            }

            continuationMeasures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        var continuationComposition = new Composition(continuationMeasures);

        compositionDecorator.Decorate(continuationComposition);

        var phrasedComposition = ApplyPhrasing(continuationComposition);

        var compositionWithEnding = endingComposer.Compose(phrasedComposition, theme);

        compositionDecorator.ApplySustain(compositionWithEnding);

        return new Composition([.. theme.Exposition, .. compositionWithEnding.Measures]);
    }

    private Composition ApplyPhrasing(Composition initialComposition)
    {
        var currentMeasureIndex = 0;
        var phrasedMeasures = new List<Measure>();

        foreach (var measure in initialComposition.Measures)
        {
            phrasedMeasures.Add(measure);

            if (currentMeasureIndex++ % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0)
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

        return phrasedComposition;
    }
}
