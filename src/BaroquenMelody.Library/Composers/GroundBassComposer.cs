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
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.Common;
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
///     When the plan modulates, each statement composes under its tonal section's key-bound components - the
///     ground renders at the relative tonic's pitch level and the upper voices walk in the relative scale's
///     grammar - and the one transition crossing a key seam validates under the arriving section's seam
///     strategy, since progression grammar is intra-key and a section boundary is exactly where it has no
///     jurisdiction. The suspension pass runs over one trailing sub-composition (its diatonic-step eligibility
///     reads identically in relative keys, which share a note set), while decoration and tonicization run over
///     per-section slices sharing chord references, so each section's figures and licenses come from its own
///     scale; every slice mechanism degenerates to the pre-modulation whole when the plan is home-only. The
///     trailing and slice shapes both keep the solo announcement exact: a suspension would otherwise happily
///     syncopate a solo stepwise ground, with no other voice present to sound the dissonance that justifies it.
/// </remarks>
internal sealed class GroundBassComposer(
    IGroundBassPlanner groundBassPlanner,
    GroundBassSectionComponents homeComponents,
    GroundBassSectionComponents? relativeComponents,
    IGroundBassDivisionScheduler groundBassDivisionScheduler,
    IVoiceRhythmLedger voiceRhythmLedger,
    ICompositionRule compositionRule,
    ISuspensionApplicator suspensionApplicator,
    ICadenceClassifier cadenceClassifier,
    IChordNumberIdentifier chordNumberIdentifier,
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

    private const int TonicChordArrivalRank = 2;

    private const int PlainArrivalRank = 3;

    public Composition Compose(CancellationToken cancellationToken)
    {
        dispatcher.Dispatch(new ResetCompositionProgress());

        // Cleared before anything composes so a repeat invocation never reads a previous composition's
        // entries; the fallback path is covered too, because the fugal composer clears again for itself.
        voiceRhythmLedger.Clear();

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
        RecordDivisionRoles(measures, plan);

        var composition = new Composition(measures);

        DecorateUpperVoices(composition, plan, cancellationToken);

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Ending));
        ApplyFinalCadence(composition, plan, cancellationToken);

        // The overall progress bar averages the theme, body, and ending progress values, so the ending must
        // report complete even though the ground's close is a single quick search - without this the bar
        // freezes at two thirds.
        dispatcher.Dispatch(new ProgressCompositionEndingProgress(100));

        cancellationToken.ThrowIfCancellationRequested();

        // The trailing sub-composition shares its measures with the full composition, so the pass mutates
        // the real chords while never seeing the solo opening statement.
        var trailingComposition = new Composition(composition.Measures.Skip(plan.MeasuresPerStatement).ToList());

        suspensionApplicator.ApplySuspensions(trailingComposition);
        ApplyTonicizationPerSection(composition, plan);
        homeComponents.Decorator.ApplySustain(composition);

        dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));
        dynamicsApplicator.Apply(composition);
        ApplyDivisionTerrace(composition, plan);

        return composition;
    }

    // Roles are recorded only here, after the walk has won and the announcement is stripped: only the
    // winning attempt's chords exist (the retry ladder rebuilds them fresh each attempt), statement s owns
    // measures [s*M, (s+1)*M), and the close does not exist yet, so it is never recorded. The stripped
    // announcement holds only ground-line notes, which record held like every other statement's.
    private void RecordDivisionRoles(List<Measure> measures, GroundBassPlan plan)
    {
        if (!groundBassDivisionScheduler.ShouldHoldGroundLine())
        {
            return;
        }

        for (var statementIndex = 0; statementIndex < plan.StatementCount; ++statementIndex)
        {
            var statementEscalates = groundBassDivisionScheduler.TryGetIntensity(plan, statementIndex, out var intensity);
            var statementHasFloridVoice = groundBassDivisionScheduler.TryGetFloridInstrument(plan, statementIndex, out var floridInstrument);

            foreach (var note in EnumerateStatementNotes(measures, plan, statementIndex))
            {
                if (note.Instrument == plan.BassInstrument)
                {
                    voiceRhythmLedger.RecordHeldNote(note);

                    continue;
                }

                if (statementEscalates)
                {
                    voiceRhythmLedger.RecordDivisionIntensity(note, intensity);
                }

                if (statementHasFloridVoice && note.Instrument == floridInstrument)
                {
                    voiceRhythmLedger.RecordFloridNote(note);
                }
            }
        }
    }

    // The terrace runs after the dynamics pass and applies exactly once per note: the dynamics engine's
    // velocity walk reads each preceding note's velocity, so an offset inside the engine would compound
    // through a statement instead of terracing it flat. Nothing draws or reads velocities after this pass,
    // and the announcement and the close sit outside every terraced statement.
    private void ApplyDivisionTerrace(Composition composition, GroundBassPlan plan)
    {
        for (var statementIndex = 0; statementIndex < plan.StatementCount; ++statementIndex)
        {
            if (!groundBassDivisionScheduler.TryGetTerraceOffset(plan, statementIndex, out var terraceOffset) || terraceOffset == 0)
            {
                continue;
            }

            foreach (var note in EnumerateStatementNotes(composition.Measures, plan, statementIndex))
            {
                ApplyTerraceOffset(note, terraceOffset);

                foreach (var ornamentationNote in note.Ornamentations)
                {
                    ApplyTerraceOffset(ornamentationNote, terraceOffset);
                }
            }
        }
    }

    private void ApplyTerraceOffset(BaroquenNote note, int terraceOffset)
    {
        var instrumentConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[note.Instrument];

        note.Velocity = new SevenBitNumber((byte)Math.Clamp(note.Velocity + terraceOffset, instrumentConfiguration.MinVelocity, instrumentConfiguration.MaxVelocity));
    }

    private static IEnumerable<BaroquenNote> EnumerateStatementNotes(IReadOnlyList<Measure> measures, GroundBassPlan plan, int statementIndex) =>
        measures
            .Skip(statementIndex * plan.MeasuresPerStatement)
            .Take(plan.MeasuresPerStatement)
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes);

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
        var pinnedNotes = new List<(Note Note, TonalSection Section)>(plan.StatementCount * plan.BassNotes.Count);

        for (var statementIndex = 0; statementIndex < plan.StatementCount; ++statementIndex)
        {
            var section = plan.SectionForStatement(statementIndex);

            pinnedNotes.AddRange(section.BassNotes.Select(note => (note, section)));
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

            var (pinnedNote, section) = pinnedNotes[pinIndex];
            var pinnedChord = CreatePinnedBassChord(plan, pinnedNote);

            // The onset after the last statement pin is the final cadence's tonic, so the walk's last onset
            // threads into the close just like every other onset threads into its successor. The close is
            // always home (the planner's tail floor), as is the final statement threading into it.
            var (nextPinnedNote, nextSection) = pinIndex + 1 < pinnedNotes.Count
                ? pinnedNotes[pinIndex + 1]
                : (plan.BassNotes[0], plan.Sections[^1]);
            var nextPinnedChord = CreatePinnedBassChord(plan, nextPinnedNote);

            // A transition whose onsets sit in different keys belongs to the ARRIVING key - its seam
            // strategy exempts the intra-key progression grammar while every voice-leading rule holds.
            var searchStrategy = IsKeyChange(pinnedNotes[pinIndex - 1].Section, section)
                ? ComponentsFor(section).SeamStrategy
                : ComponentsFor(section).FullStrategy;
            var threadStrategy = IsKeyChange(section, nextSection)
                ? ComponentsFor(nextSection).SeamStrategy
                : ComponentsFor(section).FullStrategy;

            var candidates = searchStrategy.GetRuleValidChordsForPartiallyVoicedChord([chords[^1]], pinnedChord)
                .Where(candidate => threadStrategy.HasPossibleChordForPartiallyVoicedChord([candidate], nextPinnedChord))
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

                candidatePool = searchStrategy.GetPossibleChords([chords[^1]]);
            }

            var nextChord = ComponentsFor(section).ChordSelector.SelectNextChord(chords.TakeLast(2).ToList(), candidatePool);

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
        // way the generator itself degrades when no candidate satisfies the rules. The opening statement is
        // always home (the planner's lead floor), so the home strategy voices it.
        for (var attempt = 0; attempt < MaxBootstrapAttempts; ++attempt)
        {
            var candidate = homeComponents.FullStrategy.GenerateInitialChord();
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
        // voices above it take ornamentation, and only from the moment they enter. Each section's slice is
        // decorated by its own key's engine (a home-only plan is a single slice - the pre-modulation whole),
        // and the slice boundary keeps a seam-approaching figure from reaching across the key change.
        foreach (var (section, sliceMeasures) in EnumerateSectionSlices(composition, plan, includeBoundaryAndClosingMeasures: false))
        {
            var sliceComposition = new Composition(sliceMeasures);

            foreach (var instrument in compositionConfiguration.Instruments.Where(instrument => instrument != plan.BassInstrument))
            {
                ComponentsFor(section).Decorator.Decorate(sliceComposition, instrument);
            }
        }
    }

    private void ApplyTonicizationPerSection(Composition composition, GroundBassPlan plan)
    {
        // Each section's licenses come from its own mode - the relative minor's raised leading tones are
        // what make the journey audible. Every non-final slice carries the next section's first measure as
        // a read-toward boundary (the exposition pass's idiom), so a slice-final measure keeps its
        // mid-measure site and the cross-key seam pair is a real site under the DEPARTING key's licenses -
        // sound because relative keys license identical pitch classes, and musically the classical pivot:
        // a departing chord raised into a true dominant of the arriving tonic. The boundary measure's own
        // mid-measure site stays with the slice that owns it (the pass never alters a final measure's
        // inside), so every site is drawn exactly once. The last slice carries the appended close, whose
        // final measure the pass already treats as a target only.
        foreach (var (section, sliceMeasures) in EnumerateSectionSlices(composition, plan, includeBoundaryAndClosingMeasures: true))
        {
            ComponentsFor(section).TonicizationApplicator.ApplyTonicization(new Composition(sliceMeasures));
        }
    }

    /// <summary>
    ///     Enumerates each tonal section's measure slice, sharing measure references with the composition so
    ///     the passes mutate the real chords. The first slice always excludes the solo opening statement (the
    ///     trailing idiom). With <paramref name="includeBoundaryAndClosingMeasures"/> each non-final slice
    ///     extends one measure into its successor - a boundary the harmonic pass reads toward without
    ///     altering its inside - and the last slice extends past the statements to carry the appended close;
    ///     without it, slices end at their section's edge so no figure reaches across a key change.
    /// </summary>
    private static IEnumerable<(TonalSection Section, List<Measure> Measures)> EnumerateSectionSlices(Composition composition, GroundBassPlan plan, bool includeBoundaryAndClosingMeasures)
    {
        for (var sectionIndex = 0; sectionIndex < plan.Sections.Count; ++sectionIndex)
        {
            var section = plan.Sections[sectionIndex];
            var isFinalSection = sectionIndex == plan.Sections.Count - 1;
            var firstMeasure = Math.Max(section.FirstStatement * plan.MeasuresPerStatement, plan.MeasuresPerStatement);
            var lastMeasureExclusive = (section.LastStatement + 1) * plan.MeasuresPerStatement;

            if (includeBoundaryAndClosingMeasures)
            {
                lastMeasureExclusive = isFinalSection ? composition.Measures.Count : lastMeasureExclusive + 1;
            }

            if (firstMeasure >= lastMeasureExclusive)
            {
                continue;
            }

            yield return (section, composition.Measures.Skip(firstMeasure).Take(lastMeasureExclusive - firstMeasure).ToList());
        }
    }

    private void ApplyFinalCadence(Composition composition, GroundBassPlan plan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lastStatementChord = composition.Measures[^1].Beats[^1].Chord;
        var pinnedTonicChord = CreatePinnedBassChord(plan, plan.BassNotes[0]);
        var pinnedCandidates = homeComponents.FullStrategy.GetRuleValidChordsForPartiallyVoicedChord([lastStatementChord], pinnedTonicChord);
        IReadOnlyList<BaroquenChord> candidates = pinnedCandidates;

        if (candidates.Count == 0)
        {
            logger.LogWarningMessage("No rule-valid closing chord carries the ground's tonic. Closing on the strongest free arrival instead.");

            candidates = homeComponents.FullStrategy.GetPossibleChords([lastStatementChord]).ToList();
        }

        var finalChord = SelectFinalChord(composition, candidates) ?? new BaroquenChord(lastStatementChord);

        cadentialTrillApplicator.ApplyTrill(lastStatementChord, finalChord);

        // Search candidates are born plain, but the degraded close duplicates the walk's decorated last
        // chord, whose copied ornamentation sub-notes would render after the stretched whole note and
        // desynchronize the voice - the same guard the standard ending takes on its final chord.
        finalChord.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

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

        // The statement walk emits only whole measures, so the closing measure's predecessor always holds the
        // two chords the selector's context-sensitive scoring wants.
        var lastStatementChord = composition.Measures[^1].Beats[^1].Chord;
        var bestRank = candidates.Min(candidate => RankCadence(lastStatementChord, candidate));
        var bestCandidates = candidates.Where(candidate => RankCadence(lastStatementChord, candidate) == bestRank).ToList();
        var selectorContext = new List<BaroquenChord> { composition.Measures[^1].Beats[^2].Chord, lastStatementChord };

        return homeComponents.ChordSelector.SelectNextChord(selectorContext, bestCandidates) ?? bestCandidates[0];
    }

    // The authentic ranks need the seam's chord pair to classify as V-to-I, but the walk only guarantees the
    // dominant in the BASS at a statement seam - when the upper voices leave the pair unclassifiable, every
    // pinned-tonic arrival would tie and the selector's voice-leading scoring could close on a submediant
    // color over the tonic bass. Preferring any final chord that parses as the tonic harmony keeps the
    // submediant as a true last resort.
    private int RankCadence(BaroquenChord penultimateChord, BaroquenChord finalChord) => cadenceClassifier.ClassifyCadence(penultimateChord, finalChord) switch
    {
        CadenceType.PerfectAuthentic => PerfectAuthenticCadenceRank,
        CadenceType.ImperfectAuthentic => ImperfectAuthenticCadenceRank,
        _ when chordNumberIdentifier.IdentifyChordNumber(finalChord) == ChordNumber.I => TonicChordArrivalRank,
        _ => PlainArrivalRank
    };

    private GroundBassSectionComponents ComponentsFor(TonalSection section) =>
        section.Tonic == compositionConfiguration.Tonic && section.Mode == compositionConfiguration.Mode
            ? homeComponents
            : relativeComponents ?? throw new InvalidOperationException("A foreign tonal section was planned without relative-key components.");

    private static bool IsKeyChange(TonalSection precedingSection, TonalSection section) =>
        precedingSection.Tonic != section.Tonic || precedingSection.Mode != section.Mode;

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
