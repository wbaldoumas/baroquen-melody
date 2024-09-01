using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;

namespace BaroquenMelody.Library.Compositions.Composers;

internal sealed class Composer(
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    IChordComposer chordComposer,
    IThemeComposer themeComposer,
    IEndingComposer endingComposer,
    IDispatcher dispatcher,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose()
    {
        dispatcher.Dispatch(new ResetCompositionProgress());

        var theme = ComposeMainTheme();
        var compositionBody = ComposeBodyOfComposition(theme);
        var compositionWithOrnamentation = AddOrnamentation(compositionBody);
        var compositionWithPhrasing = ApplyPhrasing(compositionWithOrnamentation, theme);
        var compositionWithEnding = ComposeEnding(compositionWithPhrasing, theme);
        var compositionWithSustain = ApplySustain(compositionWithEnding);

        return CompleteComposition(theme, compositionWithSustain);
    }

    private BaroquenTheme ComposeMainTheme()
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));

        return themeComposer.Compose();
    }

    private Composition ComposeBodyOfComposition(BaroquenTheme theme)
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Body));

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            theme.Exposition.SelectMany(static measure => measure.Beats.Select(static beat => new BaroquenChord(beat.Chord)))
        );

        var compositionBody = new List<Measure>();

        while (compositionBody.Count < compositionConfiguration.CompositionLength)
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

            compositionBody.Add(new Measure(beats, compositionConfiguration.Meter));

            DispatchProgress(compositionBody.Count);
        }

        return new Composition(compositionBody);
    }

    private void DispatchProgress(int currentMeasureCount)
    {
        dispatcher.Dispatch(new ProgressCompositionBodyProgress((double)currentMeasureCount / compositionConfiguration.CompositionLength * 100));
    }

    private Composition AddOrnamentation(Composition composition)
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ornamentation));

        compositionDecorator.Decorate(composition);

        return composition;
    }

    private Composition ApplyPhrasing(Composition initialComposition, BaroquenTheme theme)
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Phrasing));

        compositionPhraser.AddTheme(theme);

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

    private Composition ComposeEnding(Composition composition, BaroquenTheme theme)
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ending));

        return endingComposer.Compose(composition, theme);
    }

    private Composition ApplySustain(Composition composition)
    {
        compositionDecorator.ApplySustain(composition);

        return composition;
    }

    private Composition CompleteComposition(BaroquenTheme theme, Composition composition)
    {
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));

        return new Composition([.. theme.Exposition, .. composition.Measures]);
    }
}
