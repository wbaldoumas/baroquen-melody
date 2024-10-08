﻿using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
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

            logger.LogWarningMessage($"Failed to compose fugal theme attempt {attempt} of {MaxFugueCompositionAttempts}.");
        }

        logger.LogWarningMessage($"Failed to compose fugal theme after {MaxFugueCompositionAttempts} attempts.");

        var initialMeasures = ComposeInitialMeasures(cancellationToken);

        return new BaroquenTheme(initialMeasures, initialMeasures);
    }

    private void DispatchProgress(int attempt)
    {
        dispatcher.Dispatch(new ProgressCompositionThemeProgress((double)attempt / MaxFugueCompositionAttempts * 100));
    }

    private bool TryComposeFugalTheme(out BaroquenTheme? theme, CancellationToken cancellationToken)
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

        foreach (var instrument in instruments.Where(instrument => instrument != fugueSubjectInstrument))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var precedingChord = workingChords[^1];
            var nextChords = new List<BaroquenChord>();

            var transposedSubjectChords = noteTransposer.TransposeToInstrument(fugueSubject, fugueSubjectInstrument, instrument)
                .Select(static note => new BaroquenChord([note]));

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
                var otherNotes = nextChord.Notes.Where(note => note.Instrument != instrument);
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
