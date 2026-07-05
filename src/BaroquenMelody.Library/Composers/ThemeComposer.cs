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
internal sealed class ThemeComposer(
    ICompositionStrategy compositionStrategy,
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

        return new BaroquenTheme(initialMeasures, initialMeasures);
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

        foreach (var (entryIndex, instrument) in instruments.Where(instrument => instrument != fugueSubjectInstrument).Index())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // The strategy's rule context is the single preceding chord (unchanged), but the chord selector gets the
            // last two chords of the running chain so context-sensitive scoring rules (e.g. leap recovery) can fire.
            var precedingChords = workingChords.TakeLast(2).ToList();
            var nextChords = new List<BaroquenChord>();

            // Fugal entries alternate subject, answer, subject, answer...; the subject voice is the first entry, so the
            // non-subject entries at even indices state the answer (a fifth up) while odd indices restate the subject.
            var subjectOrAnswer = entryIndex % 2 == 0
                ? fugalAnswerStrategy.GenerateAnswer(fugueSubject)
                : fugueSubject;

            var placedEntryChords = fugalEntryPlacer.Place(subjectOrAnswer, instrument)
                .Select(static note => new BaroquenChord([note]));

            foreach (var placedEntryChord in placedEntryChords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords([precedingChords[^1]], placedEntryChord);
                var nextChord = chordSelector.SelectNextChord(precedingChords, possibleChords);

                if (nextChord is null)
                {
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
