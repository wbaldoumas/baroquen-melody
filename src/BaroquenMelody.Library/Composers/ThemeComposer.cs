using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Strategies;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Composers;

/// <inheritdoc cref="IThemeComposer"/>
/// <remarks>
///     The fugal exposition's pinned entry chords are validated with <paramref name="fugalEntryCompositionStrategy"/>,
///     whose rule set omits voice spacing: pinning each entry note leaves spacing near-zero candidate chords with
///     four voices, and the exposition privileges its imitative entries over spacing, as fugue expositions
///     conventionally do. Entry placement still steers toward spacing-feasible registers, and the initial measure,
///     the final bridge chord, and the rest of the composition enforce the full rule set.
/// </remarks>
internal sealed class ThemeComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionStrategy fugalEntryCompositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordComposer chordComposer,
    IFugalEntryPlacer fugalEntryPlacer,
    IFugalAnswerStrategy fugalAnswerStrategy,
    IChordSelector chordSelector,
    IDispatcher dispatcher,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : IThemeComposer
{
    private const int MaxFugueCompositionAttempts = 50;

    public BaroquenTheme Compose(CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (attempt++ < MaxFugueCompositionAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DispatchProgress(attempt);

            if (TryComposeFugalTheme(out var fugueSubject, cancellationToken))
            {
                DispatchProgress(MaxFugueCompositionAttempts);

                return fugueSubject!;
            }

            logger.LogWarningMessage($"Failed to compose fugal theme attempt {attempt} of {MaxFugueCompositionAttempts}.");
        }

        logger.LogWarningMessage($"Failed to compose fugal theme after {MaxFugueCompositionAttempts} attempts.");

        var initialMeasures = ComposeFallbackMeasures(cancellationToken);
        var fallbackComposition = new Composition(initialMeasures);

        // The fallback theme skips the fugal entries, but it should still sound like the rest of the piece: without
        // this pass its measures would be the only undecorated ones, since the theme is prepended to the final
        // composition as-is.
        foreach (var instrument in compositionConfiguration.Instruments)
        {
            compositionDecorator.Decorate(fallbackComposition, instrument);
        }

        return new BaroquenTheme(fallbackComposition.Measures, fallbackComposition.Measures);
    }

    private void DispatchProgress(int attempt)
    {
        dispatcher.Dispatch(new ProgressCompositionThemeProgress((double)attempt / MaxFugueCompositionAttempts * 100));
    }

    private bool TryComposeFugalTheme(out BaroquenTheme? theme, CancellationToken cancellationToken)
    {
        try
        {
            var initialMeasures = ComposeInitialMeasures(cancellationToken);
            var initialComposition = new Composition(initialMeasures);
            var instruments = compositionConfiguration.Instruments;
            var fugueSubjectInstrument = instruments[0];

            compositionDecorator.Decorate(initialComposition, fugueSubjectInstrument);

            var fugueSubject = initialComposition.Measures
                .SelectMany(static measure => measure.Beats)
                .Select(beat => beat.Chord[fugueSubjectInstrument])
                .ToList();

            var workingChords = initialComposition.Measures.SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)).ToList();

            workingChords = ContinueFugueSubject(fugueSubject, fugueSubjectInstrument, workingChords, instruments, cancellationToken);

            if (workingChords.Count == 0)
            {
                theme = null;
                return false;
            }

            theme = StripInstrumentsFromFugueSubject(workingChords, instruments);

            return true;
        }
        catch (NoValidChordChoicesAvailableException)
        {
            // A dead end from an unlucky starting voicing is retriable: the caller's attempt loop regenerates
            // the initial chord, so surface it as a failed attempt rather than a fatal error.
            logger.LogWarningMessage("Composition dead-ended while composing the fugal theme. Retrying from a new initial chord.");

            theme = null;
            return false;
        }
    }

    private List<Measure> ComposeFallbackMeasures(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt < MaxFugueCompositionAttempts; ++attempt)
        {
            try
            {
                return ComposeInitialMeasures(cancellationToken);
            }
            catch (NoValidChordChoicesAvailableException)
            {
                logger.LogWarningMessage($"Composition dead-ended while composing the fallback theme attempt {attempt} of {MaxFugueCompositionAttempts}.");
            }
        }

        // the final attempt runs unguarded: with every retry exhausted, a dead end here is genuinely fatal.
        return ComposeInitialMeasures(cancellationToken);
    }

    private List<Measure> ComposeInitialMeasures(CancellationToken cancellationToken)
    {
        var initialChord = compositionStrategy.GenerateInitialChord();
        var beats = new List<Beat>(compositionConfiguration.BeatsPerMeasure) { new(initialChord) };
        var precedingChords = beats.Select(static beat => beat.Chord).ToList();

        while (beats.Count < compositionConfiguration.BeatsPerMeasure)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextChord = chordComposer.Compose(precedingChords);

            precedingChords.Add(nextChord);
            beats.Add(new Beat(nextChord));
        }

        return new List<Measure>(compositionConfiguration.MinimumMeasures)
        {
            new(beats, compositionConfiguration.Meter)
        };
    }

    private List<BaroquenChord> ContinueFugueSubject(List<BaroquenNote> fugueSubject, Instrument fugueSubjectInstrument, List<BaroquenChord> workingChords, List<Instrument> instruments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var processedInstruments = new List<Instrument> { fugueSubjectInstrument };

        // Fugal entries alternate subject, answer, subject, answer...; the subject voice is the first entry, so the
        // non-subject entries at even indices state the answer (a fifth up) while odd indices restate the subject.
        // Every placement is resolved up front so each pinned beat below can see the next pinned note across entry
        // boundaries.
        var placedEntries = new List<(Instrument Instrument, List<BaroquenChord> PlacedChords)>();

        foreach (var (index, instrument) in instruments.Where(instrument => instrument != fugueSubjectInstrument).Index())
        {
            // Only the first entry knows the note its voice sounds immediately beforehand (the initial measure is
            // already composed); later entries' preceding notes are free-composed after placement must be resolved,
            // so their boundary reachability is enforced at compose time by the next-pin threading below instead.
            var placedNotes = fugalEntryPlacer.Place(
                index % 2 == 0 ? fugalAnswerStrategy.GenerateAnswer(fugueSubject) : fugueSubject,
                instrument,
                index == 0 ? workingChords[^1][instrument] : null
            );

            placedEntries.Add((instrument, placedNotes.Select(static note => new BaroquenChord([note])).ToList()));
        }

        foreach (var (entryIndex, placedEntry) in placedEntries.Index())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instrument = placedEntry.Instrument;

            // The strategy's rule context is the single preceding chord (unchanged), but the chord selector gets the
            // last two chords of the running chain so context-sensitive scoring rules (e.g. leap recovery) can fire.
            var precedingChords = workingChords.TakeLast(2).ToList();
            var nextChords = new List<BaroquenChord>();

            foreach (var (beatIndex, placedEntryChord) in placedEntry.PlacedChords.Index())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // A candidate admitting no rule-valid continuation that contains the next pinned entry note would
                // strand the entry on its very next beat, so pinned beats thread the whole line: candidates come
                // from the rule set alone (free-choice look-ahead only starves a beat whose successor is already
                // dictated) and must admit the next pin. The final pinned beat has no successor pin, so it resumes
                // the free look-ahead instead - the body composes freely from it.
                var nextPinnedChord = ResolveNextPinnedChord(placedEntries, entryIndex, beatIndex);
                var possibleChords = nextPinnedChord is null
                    ? compositionStrategy.GetPossibleChordsForPartiallyVoicedChords([precedingChords[^1]], placedEntryChord)
                    : fugalEntryCompositionStrategy.GetRuleValidChordsForPartiallyVoicedChord([precedingChords[^1]], placedEntryChord)
                        .Where(possibleChord => fugalEntryCompositionStrategy.HasPossibleChordForPartiallyVoicedChord([possibleChord], nextPinnedChord))
                        .ToList();

                var nextChord = chordSelector.SelectNextChord(precedingChords, possibleChords);

                if (nextChord is null)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebugMessage($"Fugal entry for {instrument} dead-ended at beat {beatIndex}: no rule-valid chord contains the placed entry note {placedEntryChord[instrument].Raw}.");
                    }

                    return [];
                }

                var placedEntryNote = placedEntryChord[instrument];
                var otherNotes = nextChord.Notes.Where(note => note.Instrument != instrument);
                var workingChord = new BaroquenChord([.. otherNotes, placedEntryNote]);

                nextChords.Add(workingChord);
                precedingChords.Add(workingChord);
            }

            var tempComposition = new Composition([new Measure(nextChords.Select(static chord => new Beat(chord)).ToList(), compositionConfiguration.Meter)]);

            foreach (var processedInstrument in processedInstruments)
            {
                compositionDecorator.Decorate(tempComposition, processedInstrument);
            }

            workingChords.AddRange(tempComposition.Measures.SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)));
            processedInstruments.Add(instrument);
        }

        return workingChords;
    }

    private static BaroquenChord? ResolveNextPinnedChord(List<(Instrument Instrument, List<BaroquenChord> PlacedChords)> placedEntries, int entryIndex, int beatIndex)
    {
        var placedChords = placedEntries[entryIndex].PlacedChords;

        if (beatIndex + 1 < placedChords.Count)
        {
            return placedChords[beatIndex + 1];
        }

        return entryIndex + 1 < placedEntries.Count
            ? placedEntries[entryIndex + 1].PlacedChords.FirstOrDefault()
            : null;
    }

    private BaroquenTheme StripInstrumentsFromFugueSubject(List<BaroquenChord> workingChords, List<Instrument> instruments)
    {
        var beatIndex = 0;
        var expositionMeasures = new List<Measure>();
        var recapitulationMeasures = new List<Measure>();
        var inProcessInstruments = new List<Instrument>();

        foreach (var instrument in instruments)
        {
            inProcessInstruments.Add(instrument);

            var beats = workingChords
                .Skip(beatIndex)
                .Take(compositionConfiguration.BeatsPerMeasure)
                .Select(static chord => new Beat(chord))
                .ToList();

            var strippedBeats = beats
                .Select(beat => new BaroquenChord(beat.Chord.Notes.Where(note => inProcessInstruments.Contains(note.Instrument)).ToList()))
                .Select(static newChord => new Beat(newChord))
                .ToList();

            beatIndex += compositionConfiguration.BeatsPerMeasure;

            recapitulationMeasures.Add(new Measure(beats, compositionConfiguration.Meter));
            expositionMeasures.Add(new Measure(strippedBeats, compositionConfiguration.Meter));
        }

        return new BaroquenTheme(expositionMeasures, recapitulationMeasures);
    }
}
