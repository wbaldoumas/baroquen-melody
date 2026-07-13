using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;

namespace BaroquenMelody.Library.Composers;

internal sealed class Composer(
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    IChordComposer chordComposer,
    IHarmonicRhythmScheduler harmonicRhythmScheduler,
    IThemeComposer themeComposer,
    IEndingComposer endingComposer,
    IDynamicsApplicator dynamicsApplicator,
    IDispatcher dispatcher,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose(CancellationToken cancellationToken)
    {
        dispatcher.Dispatch(new ResetCompositionProgress());

        var theme = ComposeMainTheme(cancellationToken);
        var compositionBody = ComposeBodyOfComposition(theme, cancellationToken);
        var compositionWithOrnamentation = AddOrnamentation(compositionBody, cancellationToken);
        var compositionWithPhrasing = ApplyPhrasing(compositionWithOrnamentation, theme, cancellationToken);
        var compositionWithEnding = ComposeEnding(compositionWithPhrasing, theme, cancellationToken);
        var compositionWithSustain = ApplySustain(compositionWithEnding, cancellationToken);
        var completeComposition = CompleteComposition(theme, compositionWithSustain, cancellationToken);
        var compositionWithDynamics = ApplyDynamics(completeComposition);

        return compositionWithDynamics;
    }

    private BaroquenTheme ComposeMainTheme(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));

        return themeComposer.Compose(cancellationToken);
    }

    private Composition ComposeBodyOfComposition(BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Body));

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            theme.Exposition.SelectMany(static measure => measure.Beats.Select(static beat => new BaroquenChord(beat.Chord)))
        );

        var compositionBody = new List<Measure>();

        while (compositionBody.Count < compositionConfiguration.MinimumMeasures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var beats = new List<Beat>(compositionConfiguration.BeatsPerMeasure);

            while (beats.Count < compositionConfiguration.BeatsPerMeasure)
            {
                if (beats.Count > 0 && harmonicRhythmScheduler.ShouldHoldBeat(compositionBody.Count, beats.Count))
                {
                    beats.Add(CreateHeldBeat(beats[^1].Chord));

                    continue;
                }

                var nextChord = chordComposer.Compose(compositionContext);

                compositionContext.Add(nextChord);
                beats.Add(new Beat(nextChord));
            }

            compositionBody.Add(new Measure(beats, compositionConfiguration.Meter));

            DispatchProgress(compositionBody.Count);
        }

        return new Composition(compositionBody);
    }

    /// <summary>
    ///     Builds a beat that holds the preceding beat's harmony: the preceding chord's notes become sustain
    ///     principals spanning both beats, and the held copy's notes become the silent mid-sustain continuations
    ///     the renderer covers with the principal - mirroring exactly what the sustain pass produces for repeated
    ///     notes. Held beats duplicate an already rule-valid chord, so no new vertical sonorities are introduced,
    ///     and they are deliberately kept out of the harmonic context so the repetition rules judge the true
    ///     sounding transition from the held harmony to the next composed chord.
    /// </summary>
    private Beat CreateHeldBeat(BaroquenChord precedingChord)
    {
        var heldChord = new BaroquenChord(precedingChord);

        foreach (var note in heldChord.Notes)
        {
            note.OrnamentationType = OrnamentationType.MidSustain;
        }

        foreach (var note in precedingChord.Notes)
        {
            note.MusicalTimeSpan = compositionConfiguration.DefaultNoteTimeSpan + compositionConfiguration.DefaultNoteTimeSpan;
            note.OrnamentationType = OrnamentationType.Sustain;
        }

        return new Beat(heldChord);
    }

    private void DispatchProgress(int currentMeasureCount)
    {
        dispatcher.Dispatch(new ProgressCompositionBodyProgress((double)currentMeasureCount / compositionConfiguration.MinimumMeasures * 100));
    }

    private Composition AddOrnamentation(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ornamentation));

        compositionDecorator.Decorate(composition);

        return composition;
    }

    private Composition ApplyPhrasing(Composition initialComposition, BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Phrasing));

        compositionPhraser.AddTheme(theme);

        var phrasedMeasures = new List<Measure>();

        foreach (var (currentMeasureIndex, measure) in initialComposition.Measures.Index())
        {
            cancellationToken.ThrowIfCancellationRequested();

            phrasedMeasures.Add(measure);

            if (currentMeasureIndex % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0)
            {
                compositionPhraser.AttemptPhraseRepetition(phrasedMeasures);
            }

            if (phrasedMeasures.Count >= compositionConfiguration.MinimumMeasures)
            {
                break;
            }
        }

        var phrasedComposition = new Composition(phrasedMeasures);

        compositionDecorator.Decorate(phrasedComposition);

        return phrasedComposition;
    }

    private Composition ComposeEnding(Composition composition, BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ending));

        return endingComposer.Compose(composition, theme, cancellationToken);
    }

    private Composition ApplySustain(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        compositionDecorator.ApplySustain(composition);

        return composition;
    }

    private Composition CompleteComposition(BaroquenTheme theme, Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));

        return new Composition([.. theme.Exposition, .. composition.Measures]);
    }

    private Composition ApplyDynamics(Composition composition)
    {
        dynamicsApplicator.Apply(composition);

        return composition;
    }
}
