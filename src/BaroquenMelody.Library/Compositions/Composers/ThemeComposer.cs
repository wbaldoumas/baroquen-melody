using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IThemeComposer"/>
internal sealed class ThemeComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordComposer chordComposer,
    INoteTransposer noteTransposer,
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

            logger.FailedToComposeFugalThemeAttempt(attempt, MaxFugueCompositionAttempts);
        }

        logger.FailedToComposeFugalTheme(MaxFugueCompositionAttempts);

        var initialMeasures = ComposeInitialMeasures(cancellationToken);

        return new BaroquenTheme(initialMeasures, initialMeasures);
    }

    private void DispatchProgress(int attempt)
    {
        dispatcher.Dispatch(new ProgressCompositionThemeProgress(((double)attempt / MaxFugueCompositionAttempts) * 100));
    }

    private bool TryComposeFugalTheme(out BaroquenTheme? theme, CancellationToken cancellationToken)
    {
        var initialMeasures = ComposeInitialMeasures(cancellationToken);
        var initialComposition = new Composition(initialMeasures);

        var instruments = compositionConfiguration.InstrumentConfigurations
            .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
            .Select(static instrumentConfiguration => instrumentConfiguration.Instrument)
            .ToList();

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

        return new List<Measure>(compositionConfiguration.CompositionLength)
        {
            new(beats, compositionConfiguration.Meter)
        };
    }

    private List<BaroquenChord> ContinueFugueSubject(List<BaroquenNote> fugueSubject, Instrument fugueSubjectInstrument, List<BaroquenChord> workingChords, List<Instrument> instruments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var processedInstruments = new List<Instrument> { fugueSubjectInstrument };

        foreach (var instrument in instruments.Where(instrument => instrument != fugueSubjectInstrument))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var precedingChord = workingChords[^1];
            var nextChords = new List<BaroquenChord>();

            var transposedSubjectChords = noteTransposer.TransposeToInstrument(fugueSubject, fugueSubjectInstrument, instrument)
                .Select(static note => new BaroquenChord([note]))
                .ToList();

            foreach (var transposedSubjectChord in transposedSubjectChords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords([precedingChord], transposedSubjectChord);

                if (possibleChords.Count == 0)
                {
                    return [];
                }

                var nextChord = possibleChords.OrderBy(static _ => ThreadLocalRandom.Next()).First();
                var transposedSubjectNote = transposedSubjectChord[instrument];
                var otherNotes = nextChord.Notes.Where(note => note.Instrument != instrument).ToList();
                var workingChord = new BaroquenChord([.. otherNotes, transposedSubjectNote]);

                nextChords.Add(workingChord);
                precedingChord = workingChord;
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
