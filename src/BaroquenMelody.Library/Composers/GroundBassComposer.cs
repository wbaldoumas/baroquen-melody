using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Strategies;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Composers;

/// <inheritdoc cref="IComposer"/>
/// <remarks>
///     Composes in ground bass form: the planned ground announces itself alone in the opening statement, then
///     repeats under a full texture whose upper voices are searched fresh at every ground-note onset with the
///     bass pinned, so each statement harmonizes the same bass anew. Every onset threads to the next pin the
///     way the fugal exposition threads its entries, and the held slot of each ground note is a plain duplicate
///     that the ornamentation and sustain passes animate. A dead-ended walk retries from fresh draws; the final
///     attempt may take a local liberty and compose a starving onset unpinned (a varied ground, historically
///     legitimate), and only a bass range that cannot host any ground at all falls back to the standard form.
///     The suspension and tonicization passes run over a trailing sub-composition that shares chord references
///     with the full composition (the completion pass's exposition idiom), which keeps the solo announcement
///     exact: a suspension would otherwise happily syncopate a solo stepwise ground, with no other voice
///     present to sound the dissonance that justifies it.
/// </remarks>
internal sealed class GroundBassComposer(
    IGroundBassPlanner groundBassPlanner,
    ICompositionStrategy compositionStrategy,
    ICompositionRule compositionRule,
    IChordSelector chordSelector,
    ICompositionDecorator compositionDecorator,
    ISuspensionApplicator suspensionApplicator,
    ITonicizationApplicator tonicizationApplicator,
    ICadenceClassifier cadenceClassifier,
    ICadentialTrillApplicator cadentialTrillApplicator,
    IDynamicsApplicator dynamicsApplicator,
    IComposer fallbackComposer,
    IDispatcher dispatcher,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    private const int MaxCompositionAttempts = 10;

    private const int MaxBootstrapAttempts = 50;

    private const int PerfectAuthenticCadenceRank = 0;

    private const int ImperfectAuthenticCadenceRank = 1;

    private const int PlainArrivalRank = 2;

    public Composition Compose(CancellationToken cancellationToken)
    {
        dispatcher.Dispatch(new ResetCompositionProgress());

        var plan = groundBassPlanner.CreatePlan();

        if (plan is null)
        {
            logger.LogWarningMessage("No ground bass pattern can anchor inside the bass instrument's range. Falling back to the standard form.");

            return fallbackComposer.Compose(cancellationToken);
        }

        // The form has no fugal exposition, so the theme step reports complete immediately and the
        // progress display moves straight to the body.
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));
        dispatcher.Dispatch(new ProgressCompositionThemeProgress(100));
        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Body));

        var statementChords = ComposeStatements(plan, cancellationToken);

        if (statementChords is null)
        {
            logger.LogWarningMessage("Ground bass statements could not be composed after every retry. Falling back to the standard form.");

            return fallbackComposer.Compose(cancellationToken);
        }

        var measures = ConvertChordsToMeasures(statementChords);

        StripOpeningStatementToTheGround(measures, plan);

        var composition = new Composition(measures);

        DecorateUpperVoices(composition, plan, cancellationToken);

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ending));
        ApplyFinalCadence(composition, plan, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // The trailing sub-composition shares its measures with the full composition, so the passes mutate
        // the real chords while never seeing the solo opening statement.
        var trailingComposition = new Composition(composition.Measures.Skip(plan.MeasuresPerStatement).ToList());

        suspensionApplicator.ApplySuspensions(trailingComposition);
        tonicizationApplicator.ApplyTonicization(trailingComposition);
        compositionDecorator.ApplySustain(composition);

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));
        dynamicsApplicator.Apply(composition);

        return composition;
    }

    private List<BaroquenChord>? ComposeStatements(GroundBassPlan plan, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxCompositionAttempts; ++attempt)
        {
            if (TryComposeStatements(plan, attempt == MaxCompositionAttempts, out var statementChords, cancellationToken))
            {
                return statementChords;
            }

            logger.LogWarningMessage($"Ground bass composition dead-ended on attempt {attempt} of {MaxCompositionAttempts}. Retrying from fresh draws.");
        }

        return null;
    }

    private bool TryComposeStatements(GroundBassPlan plan, bool allowUnpinnedSites, out List<BaroquenChord>? statementChords, CancellationToken cancellationToken)
    {
        var pinnedNotes = new List<Note>(plan.StatementCount * plan.BassNotes.Count);

        for (var statementIndex = 0; statementIndex < plan.StatementCount; ++statementIndex)
        {
            pinnedNotes.AddRange(plan.BassNotes);
        }

        var bootstrapChord = BootstrapInitialChord(plan);
        var chords = new List<BaroquenChord>(pinnedNotes.Count * GroundBassPlan.SlotsPerGroundNote)
        {
            bootstrapChord,
            new(bootstrapChord)
        };

        for (var pinIndex = 1; pinIndex < pinnedNotes.Count; ++pinIndex)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pinnedChord = CreatePinnedBassChord(plan, pinnedNotes[pinIndex]);

            // The onset after the last statement pin is the final cadence's tonic, so the walk's last onset
            // threads into the close just like every other onset threads into its successor.
            var nextPinnedNote = pinIndex + 1 < pinnedNotes.Count ? pinnedNotes[pinIndex + 1] : plan.BassNotes[0];
            var nextPinnedChord = CreatePinnedBassChord(plan, nextPinnedNote);

            var candidates = compositionStrategy.GetRuleValidChordsForPartiallyVoicedChord([chords[^1]], pinnedChord)
                .Where(candidate => compositionStrategy.HasPossibleChordForPartiallyVoicedChord([candidate], nextPinnedChord))
                .ToList();

            IEnumerable<BaroquenChord> candidatePool = candidates;

            if (candidates.Count == 0)
            {
                if (!allowUnpinnedSites)
                {
                    statementChords = null;

                    return false;
                }

                logger.LogWarningMessage($"No rule-valid chord carries the pinned ground note at onset {pinIndex}. Taking a local liberty and composing the onset unpinned.");

                candidatePool = compositionStrategy.GetPossibleChords([chords[^1]]);
            }

            var nextChord = chordSelector.SelectNextChord(chords.TakeLast(2).ToList(), candidatePool);

            if (nextChord is null)
            {
                statementChords = null;

                return false;
            }

            chords.Add(nextChord);
            chords.Add(new BaroquenChord(nextChord));

            dispatcher.Dispatch(new ProgressCompositionBodyProgress((double)(pinIndex + 1) / pinnedNotes.Count * 100));
        }

        statementChords = chords;

        return true;
    }

    private BaroquenChord BootstrapInitialChord(GroundBassPlan plan)
    {
        BaroquenChord pinnedChord = null!;

        // The initial voicing generator knows nothing about the ground, so pin the bass onto each candidate
        // and validate the result with an empty preceding-chord context, degrading to the last candidate the
        // way the generator itself degrades when no candidate satisfies the rules.
        for (var attempt = 0; attempt < MaxBootstrapAttempts; ++attempt)
        {
            var candidate = compositionStrategy.GenerateInitialChord();
            var upperNotes = candidate.Notes.Where(note => note.Instrument != plan.BassInstrument);

            pinnedChord = new BaroquenChord([.. upperNotes, new BaroquenNote(plan.BassInstrument, plan.BassNotes[0], compositionConfiguration.DefaultNoteTimeSpan)]);

            if (compositionRule.Evaluate([], pinnedChord))
            {
                return pinnedChord;
            }
        }

        logger.LogWarningMessage($"No rule-compliant opening voicing carries the ground's first note after {MaxBootstrapAttempts} attempts. Using the last candidate.");

        return pinnedChord;
    }

    private void StripOpeningStatementToTheGround(List<Measure> measures, GroundBassPlan plan)
    {
        for (var measureIndex = 0; measureIndex < plan.MeasuresPerStatement; ++measureIndex)
        {
            var strippedBeats = measures[measureIndex].Beats
                .Select(beat => new Beat(new BaroquenChord([beat.Chord[plan.BassInstrument]])))
                .ToList();

            measures[measureIndex] = new Measure(strippedBeats, compositionConfiguration.Meter);
        }
    }

    private void DecorateUpperVoices(Composition composition, GroundBassPlan plan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ornamentation));

        // The ground itself stays plain so the anchor remains recognizable through every variation; only the
        // voices above it take ornamentation, and only from the moment they enter.
        var textureComposition = new Composition(composition.Measures.Skip(plan.MeasuresPerStatement).ToList());

        foreach (var instrument in compositionConfiguration.Instruments.Where(instrument => instrument != plan.BassInstrument))
        {
            compositionDecorator.Decorate(textureComposition, instrument);
        }
    }

    private void ApplyFinalCadence(Composition composition, GroundBassPlan plan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lastStatementChord = composition.Measures[^1].Beats[^1].Chord;
        var pinnedTonicChord = CreatePinnedBassChord(plan, plan.BassNotes[0]);
        var pinnedCandidates = compositionStrategy.GetRuleValidChordsForPartiallyVoicedChord([lastStatementChord], pinnedTonicChord);
        IReadOnlyList<BaroquenChord> candidates = pinnedCandidates;

        if (candidates.Count == 0)
        {
            logger.LogWarningMessage("No rule-valid closing chord carries the ground's tonic. Closing on the strongest free arrival instead.");

            candidates = compositionStrategy.GetPossibleChords([lastStatementChord]).ToList();
        }

        var finalChord = SelectFinalChord(composition, candidates) ?? new BaroquenChord(lastStatementChord);

        cadentialTrillApplicator.ApplyTrill(lastStatementChord, finalChord);

        foreach (var note in finalChord.Notes)
        {
            note.MusicalTimeSpan = MusicalTimeSpan.Whole;
        }

        var restingChord = new BaroquenChord(finalChord);

        restingChord.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

        foreach (var note in restingChord.Notes)
        {
            note.OrnamentationType = OrnamentationType.Rest;
        }

        composition.Measures.Add(new Measure([new Beat(finalChord), new Beat(restingChord)], compositionConfiguration.Meter));
    }

    private BaroquenChord? SelectFinalChord(Composition composition, IReadOnlyList<BaroquenChord> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var lastStatementChord = composition.Measures[^1].Beats[^1].Chord;
        var bestRank = candidates.Min(candidate => RankCadence(lastStatementChord, candidate));
        var bestCandidates = candidates.Where(candidate => RankCadence(lastStatementChord, candidate) == bestRank).ToList();
        var selectorContext = composition.Measures[^1].Beats.Count >= 2
            ? new List<BaroquenChord> { composition.Measures[^1].Beats[^2].Chord, lastStatementChord }
            : [lastStatementChord];

        return chordSelector.SelectNextChord(selectorContext, bestCandidates) ?? bestCandidates[0];
    }

    private int RankCadence(BaroquenChord penultimateChord, BaroquenChord finalChord) => cadenceClassifier.ClassifyCadence(penultimateChord, finalChord) switch
    {
        CadenceType.PerfectAuthentic => PerfectAuthenticCadenceRank,
        CadenceType.ImperfectAuthentic => ImperfectAuthenticCadenceRank,
        _ => PlainArrivalRank
    };

    private BaroquenChord CreatePinnedBassChord(GroundBassPlan plan, Note pinnedNote) =>
        new([new BaroquenNote(plan.BassInstrument, pinnedNote, compositionConfiguration.DefaultNoteTimeSpan)]);

    private List<Measure> ConvertChordsToMeasures(IReadOnlyList<BaroquenChord> chords)
    {
        var beats = new List<Beat>();
        var measures = new List<Measure>();

        foreach (var chord in chords)
        {
            beats.Add(new Beat(chord));

            if (beats.Count != compositionConfiguration.BeatsPerMeasure)
            {
                continue;
            }

            measures.Add(new Measure([.. beats], compositionConfiguration.Meter));
            beats = [];
        }

        return measures;
    }
}
