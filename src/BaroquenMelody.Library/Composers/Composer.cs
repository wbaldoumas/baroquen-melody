using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Rhythm.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Composers;

internal sealed class Composer(
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    IChordComposer chordComposer,
    IHarmonicRhythmScheduler harmonicRhythmScheduler,
    IVoiceRhythmScheduler voiceRhythmScheduler,
    IVoiceRhythmLedger voiceRhythmLedger,
    ISuspensionApplicator suspensionApplicator,
    ITonicizationApplicator tonicizationApplicator,
    IThemeComposer themeComposer,
    IEndingComposer endingComposer,
    IDynamicsApplicator dynamicsApplicator,
    IDispatcher dispatcher,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    // One metric-accent level below the melody; a single named ear-tunable constant.
    private const int AccompanimentVelocityOffset = -8;

    public Composition Compose(CancellationToken cancellationToken)
    {
        dispatcher.Dispatch(new ResetCompositionProgress());

        // Cleared before the theme rather than at the body step: the theme's decoration pass reads the same
        // gates, and a reused composer instance must never weight a fresh exposition by a previous
        // composition's stale marks.
        voiceRhythmLedger.Clear();

        var theme = ComposeMainTheme(cancellationToken);
        var compositionBody = ComposeBodyOfComposition(theme, cancellationToken);
        var compositionWithOrnamentation = AddOrnamentation(compositionBody, cancellationToken);
        var compositionWithPhrasing = ApplyPhrasing(compositionWithOrnamentation, theme, cancellationToken);
        var compositionWithEnding = ComposeEnding(compositionWithPhrasing, theme, cancellationToken);
        var compositionWithSuspensions = ApplySuspensions(compositionWithEnding, cancellationToken);
        var compositionWithTonicization = ApplyTonicization(compositionWithSuspensions, cancellationToken);
        var compositionWithSustain = ApplySustain(compositionWithTonicization, cancellationToken);
        var completeComposition = CompleteComposition(theme, compositionWithSustain, cancellationToken);
        var compositionWithDynamics = ApplyDynamics(completeComposition);

        ApplyTextureProminence(compositionWithDynamics);

        return compositionWithDynamics;
    }

    // The melody's florid mark is a statistical tilt, not a concentration, so the accompaniment could
    // out-figure it; prominence is therefore carried by velocity. Runs after the dynamics pass and applies
    // exactly once per note (an in-engine offset would compound through the velocity walk - the terrace
    // precedent), draws nothing, and nothing reads velocities after it. Gated on the scheduler's texture
    // answer: outside a texture the held store carries the rotation's roles, which must never be offset.
    // Under a texture the held store holds exactly the pads, and the exposition's notes are unrecorded, so
    // the frame and the melody keep their full voice.
    private void ApplyTextureProminence(Composition composition)
    {
        if (!voiceRhythmScheduler.TryGetTextureDecorationOrder(out _))
        {
            return;
        }

        var accompanimentNotes = composition.Measures
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes)
            .Where(note => voiceRhythmLedger.IsHeldNote(note) || voiceRhythmLedger.IsTextureFigurationNote(note));

        foreach (var note in accompanimentNotes)
        {
            ApplyProminenceOffset(note);

            foreach (var ornamentationNote in note.Ornamentations)
            {
                ApplyProminenceOffset(ornamentationNote);
            }
        }
    }

    private void ApplyProminenceOffset(BaroquenNote note)
    {
        var instrumentConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[note.Instrument];

        note.Velocity = new SevenBitNumber((byte)Math.Clamp(note.Velocity + AccompanimentVelocityOffset, instrumentConfiguration.MinVelocity, instrumentConfiguration.MaxVelocity));
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
                // A held beat is a plain duplicate of the preceding chord: it enters the harmonic context so the
                // repetition rules force the next composed beat onto a new harmony, and it flows through the
                // ornamentation and sustain passes like any repeated chord - each voice is independently
                // ornamented, tied into a two-beat sustain, or re-articulated over the held harmony. Duplicating
                // an already rule-valid chord introduces no new vertical sonorities.
                if (beats.Count > 0 && harmonicRhythmScheduler.ShouldHoldBeat(compositionBody.Count, beats.Count))
                {
                    var heldChord = new BaroquenChord(beats[^1].Chord);

                    compositionContext.Add(heldChord);
                    beats.Add(new Beat(heldChord));

                    continue;
                }

                var nextChord = ComposeNextChord(compositionContext, compositionBody.Count, beats);

                compositionContext.Add(nextChord);
                beats.Add(new Beat(nextChord));
            }

            RecordVoiceRhythmRoles(compositionBody.Count, beats);

            compositionBody.Add(new Measure(beats, compositionConfiguration.Meter));

            DispatchProgress(compositionBody.Count);
        }

        return new Composition(compositionBody);
    }

    // A held voice's pin is its own note from the previous beat: honoring it keeps the voice on a common
    // tone through the mid-measure harmonic change, and the pin can only fire at interior beats, so the
    // previous beat always exists.
    private BaroquenChord ComposeNextChord(FixedSizeList<BaroquenChord> compositionContext, int measureIndex, List<Beat> beats) =>
        beats.Count > 0
        && voiceRhythmScheduler.TryGetPinnedInstrument(measureIndex, beats.Count, out var pinnedInstrument)
        && beats[^1].Chord.ContainsInstrument(pinnedInstrument)
            ? chordComposer.Compose(compositionContext, beats[^1].Chord[pinnedInstrument])
            : chordComposer.Compose(compositionContext);

    // Each scheduler answer gates only its own recording - the rotation's held and florid marks and the
    // texture's static assignment are independent contract answers, and the scheduler guarantees the two
    // modes never answer together (an active texture declines the rotation entirely).
    private void RecordVoiceRhythmRoles(int measureIndex, List<Beat> beats)
    {
        if (voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out var heldInstrument))
        {
            foreach (var beat in beats.Where(beat => beat.Chord.ContainsInstrument(heldInstrument)))
            {
                voiceRhythmLedger.RecordHeldNote(beat.Chord[heldInstrument]);
            }
        }

        if (voiceRhythmScheduler.TryGetFloridInstrument(measureIndex, out var floridInstrument))
        {
            foreach (var beat in beats.Where(beat => beat.Chord.ContainsInstrument(floridInstrument)))
            {
                voiceRhythmLedger.RecordFloridNote(beat.Chord[floridInstrument]);
            }
        }

        foreach (var instrument in compositionConfiguration.Instruments)
        {
            if (!voiceRhythmScheduler.TryGetTextureRole(instrument, out var textureRole))
            {
                continue;
            }

            foreach (var beat in beats.Where(beat => beat.Chord.ContainsInstrument(instrument)))
            {
                RecordTextureRole(textureRole, beat.Chord[instrument]);
            }
        }
    }

    // The melody rides the florid store (the subdividing tier tilts toward it) and the pads ride the held
    // store (tied wherever their pairs repeat), so two of the three texture roles ARE the shipped roles;
    // the figuration voice carries its own mark, and pads carry one besides the held record so the
    // gentle-figure gate can tell them from the rotation's held voice and the ground's tread - the other
    // held-store tenants, which must stay fully ornament-silenced.
    private void RecordTextureRole(TextureRole textureRole, BaroquenNote note)
    {
        switch (textureRole)
        {
            case TextureRole.Melody:
                voiceRhythmLedger.RecordFloridNote(note);
                break;
            case TextureRole.Figuration:
                voiceRhythmLedger.RecordTextureFigurationNote(note);
                break;
            case TextureRole.Pad:
                voiceRhythmLedger.RecordHeldNote(note);
                voiceRhythmLedger.RecordTexturePadNote(note);
                break;
            default:
                // The same discipline the transformer's texture classification enforces: a future role must
                // decide its stores deliberately, never inherit the pad's by fall-through.
                throw new InvalidOperationException("The texture role has no store classification.");
        }
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

        RecordTextureRolesForRestatements(phrasedComposition);

        compositionDecorator.Decorate(phrasedComposition);

        return phrasedComposition;
    }

    // The phraser's restatements are deep copies, so they arrive here carrying no ledger marks: left
    // unrecorded, the redecoration below would re-fill exactly the gaps the texture carved (at stock
    // weights) and the prominence pass would skip the copies - the texture would flicker off for every
    // restatement. Texture roles are static per instrument, so each copy simply takes its instrument's
    // role, and re-recording the walked originals is an idempotent no-op on the reference-keyed stores.
    // An accompaniment copy also sheds any copied-in figures before it is recorded (a theme-phrase
    // restatement carries the exposition's stock decoration by value), so the redecoration re-clothes it
    // in the texture's own fabric; the melody keeps its copied figures - thematic recall is its
    // privilege. Outside a texture the scheduler answers no roles and the loop touches nothing.
    private void RecordTextureRolesForRestatements(Composition composition)
    {
        foreach (var instrument in compositionConfiguration.Instruments)
        {
            if (!voiceRhythmScheduler.TryGetTextureRole(instrument, out var textureRole))
            {
                continue;
            }

            foreach (var beat in composition.Measures.SelectMany(static measure => measure.Beats).Where(beat => beat.Chord.ContainsInstrument(instrument)))
            {
                var note = beat.Chord[instrument];

                if (textureRole != TextureRole.Melody && IsUnrecordedAccompanimentCopy(textureRole, note))
                {
                    note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
                }

                RecordTextureRole(textureRole, note);
            }
        }
    }

    // Consulted only for the pad and figuration roles: the melody never sheds figures, so its store is
    // never read here. A note absent from its role's OWN store at this point in the pipeline can only be
    // a phraser copy - every walked body note was recorded as it was composed, pads into the pad store.
    private bool IsUnrecordedAccompanimentCopy(TextureRole textureRole, BaroquenNote note) => textureRole == TextureRole.Figuration
        ? !voiceRhythmLedger.IsTextureFigurationNote(note)
        : !voiceRhythmLedger.IsTexturePadNote(note);

    private Composition ComposeEnding(Composition composition, BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ending));

        return endingComposer.Compose(composition, theme, cancellationToken);
    }

    // Suspensions run after the ending is composed, so every eligible pair is still plain default-span
    // material, and before the sustain pass, which may absorb a preparation into a longer backward tie.
    private Composition ApplySuspensions(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        suspensionApplicator.ApplySuspensions(composition);

        return composition;
    }

    // Tonicization runs after the suspension pass, so a raised third can never contradict a stamped
    // suspension figure, and before the sustain pass, so every obligation check reads actual sounded
    // notes. Eligibility is judged on diatonic chord numbers; only pitches change, never the walk.
    private Composition ApplyTonicization(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        tonicizationApplicator.ApplyTonicization(composition);

        return composition;
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

        // The exposition joins the composition only here, so it takes its own suspension and
        // tonicization passes now. Suspensions are a pure time-shift and tonicization only raises
        // resolved thirds, which keeps every voice's fugal entry recognizable. The first body measure
        // rides along solely as the walk's boundary: the suspension pass's final-measure rule leaves it
        // untouched, and the tonicization pass only ever reads it as a resolution target, so the seam
        // stays coherent and no site is ever drawn twice.
        var expositionWithBoundary = new Composition([.. theme.Exposition, composition.Measures[0]]);

        suspensionApplicator.ApplySuspensions(expositionWithBoundary);
        tonicizationApplicator.ApplyTonicization(expositionWithBoundary);

        return new Composition([.. theme.Exposition, .. composition.Measures]);
    }

    private Composition ApplyDynamics(Composition composition)
    {
        dynamicsApplicator.Apply(composition);

        return composition;
    }
}
