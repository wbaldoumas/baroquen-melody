using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Collections.Extensions;
using BaroquenMelody.Library.Infrastructure.Exceptions;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IEndingComposer"/>
internal sealed class EndingComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordNumberIdentifier chordNumberIdentifier,
    IDispatcher dispatcher,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : IEndingComposer
{
    private const int MaxBridgingChords = 25;

    private const int MaxChordsToTonic = 25;

    public Composition Compose(Composition composition, BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var bridgingComposition = GetBridgingComposition(composition, theme, cancellationToken);

        composition.Measures[^1].Beats[^1] = bridgingComposition.Measures[0].Beats[0];

        var continuationChords = bridgingComposition.Measures
            .SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord))
            .TrimEdges()
            .Concat(theme.Recapitulation.SelectMany(static measure => measure.Beats.Select(static beat => new BaroquenChord(beat.Chord))));

        foreach (var measure in ConvertChordsToMeasures(continuationChords))
        {
            composition.Measures.Add(measure);
        }

        ApplyFinalCadence(composition, cancellationToken);

        return composition;
    }

    private Composition GetBridgingComposition(Composition composition, BaroquenTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var firstChordOfRecapitulation = new BaroquenChord(theme.Recapitulation[0].Beats[0].Chord);

        firstChordOfRecapitulation.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

        var bridgingChords = GetBridgingChords(composition, firstChordOfRecapitulation, cancellationToken);
        var lastChordOfComposition = new BaroquenChord(composition.Measures[^1].Beats[^1].Chord);

        lastChordOfComposition.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

        var chords = new List<BaroquenChord> { lastChordOfComposition };

        chords.AddRange(bridgingChords);
        chords.Add(firstChordOfRecapitulation);

        var measures = ConvertChordsToMeasures(chords);
        var bridgingComposition = new Composition(measures);

        compositionDecorator.Decorate(bridgingComposition);

        return bridgingComposition;
    }

    private List<BaroquenChord> GetBridgingChords(Composition composition, BaroquenChord firstChordOfRecapitulation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            composition.Measures.SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord))
        );

        var chords = new List<BaroquenChord>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DispatchBridgingChordsProgress(chords.Count);

            var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords(compositionContext, firstChordOfRecapitulation);

            if (possibleChords.Count > 0)
            {
                break;
            }

            var nextChord = GetBridgingChord(compositionContext, firstChordOfRecapitulation, cancellationToken);

            compositionContext.Add(nextChord);
            chords.Add(nextChord);

            if (chords.Count < MaxBridgingChords)
            {
                logger.ComposedBridgingChord(chords.Count, MaxBridgingChords);

                continue;
            }

            logger.CouldNotFindSuitableBridgingChord(MaxBridgingChords);

            break;
        }

        DispatchBridgingChordsProgress(MaxBridgingChords);

        return chords;
    }

    private void DispatchBridgingChordsProgress(int currentChordCount)
    {
        var progress = (double)currentChordCount / MaxBridgingChords * 100 / 2;

        dispatcher.Dispatch(new ProgressCompositionEndingProgress(progress));
    }

    private BaroquenChord GetBridgingChord(IReadOnlyList<BaroquenChord> compositionContext, BaroquenChord targetChord, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(compositionContext);

        foreach (var chordChoice in possibleChordChoices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var workingCompositionContext = new FixedSizeList<BaroquenChord>(compositionConfiguration.CompositionContextSize);

            foreach (var chord in compositionContext)
            {
                workingCompositionContext.Add(chord);
            }

            var possibleChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice, compositionConfiguration.DefaultNoteTimeSpan);

            workingCompositionContext.Add(possibleChord);

            var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords(workingCompositionContext, targetChord);

            if (possibleChords.Count <= 0)
            {
                continue;
            }

            return possibleChord;
        }

        return GetNextChord(possibleChordChoices, compositionContext);
    }

    private void ApplyFinalCadence(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (chordNumberIdentifier.IdentifyChordNumber(composition.Measures[^1].Beats[^1].Chord) != ChordNumber.I)
        {
            var compositionWithTonicFinalChord = GetCompositionWithTonicFinalChord(composition, cancellationToken);

            composition.Measures[^1].Beats[^1] = compositionWithTonicFinalChord.Measures[0].Beats[0];

            var continuationChords = compositionWithTonicFinalChord.Measures
                .SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord))
                .Skip(1)
                .ToList();

            foreach (var measure in ConvertChordsToMeasures(continuationChords))
            {
                composition.Measures.Add(measure);
            }
        }

        DispatchChordsToTonicProgress(MaxChordsToTonic);

        var finalChordOfComposition = composition.Measures[^1].Beats[^1].Chord;

        finalChordOfComposition.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

        foreach (var note in finalChordOfComposition.Notes)
        {
            note.MusicalTimeSpan = MusicalTimeSpan.Whole;
        }

        var restingChord = new BaroquenChord(finalChordOfComposition);

        restingChord.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);

        foreach (var note in restingChord.Notes)
        {
            note.OrnamentationType = OrnamentationType.Rest;
        }

        composition.Measures[^1].Beats.Add(new Beat(restingChord));
    }

    private Composition GetCompositionWithTonicFinalChord(Composition composition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            composition.Measures.SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord))
        );

        var lastChordOfComposition = compositionContext[^1];
        var chords = new List<BaroquenChord> { lastChordOfComposition };

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DispatchChordsToTonicProgress(chords.Count);

            var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(compositionContext);

            if (TryFindTonicChord(possibleChordChoices, compositionContext, chords, cancellationToken))
            {
                break;
            }

            var nextChord = GetNextChord(possibleChordChoices, compositionContext);

            chords.Add(nextChord);
            compositionContext.Add(nextChord);

            if (chords.Count < MaxChordsToTonic)
            {
                logger.ComposedChordToTonic(chords.Count, MaxChordsToTonic);

                continue;
            }

            logger.CouldNotFindTonicChord(MaxChordsToTonic);

            break;
        }

        var compositionWithTonicFinalChord = new Composition(ConvertChordsToMeasures(chords));

        compositionDecorator.Decorate(compositionWithTonicFinalChord);

        return compositionWithTonicFinalChord;
    }

    private bool TryFindTonicChord(
        IReadOnlyList<ChordChoice> possibleChordChoices,
        FixedSizeList<BaroquenChord> compositionContext,
        List<BaroquenChord> chords,
        CancellationToken cancellationToken)
    {
        var foundTonicChord = false;

        foreach (var possibleChordChoice in possibleChordChoices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var potentialTonicChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, possibleChordChoice, compositionConfiguration.DefaultNoteTimeSpan);

            if (chordNumberIdentifier.IdentifyChordNumber(potentialTonicChord) != ChordNumber.I)
            {
                continue;
            }

            chords.Add(potentialTonicChord);
            foundTonicChord = true;

            break;
        }

        return foundTonicChord;
    }

    private void DispatchChordsToTonicProgress(int currentChordCount)
    {
        var progress = (double)currentChordCount / MaxChordsToTonic * 100 / 2 + 50;

        dispatcher.Dispatch(new ProgressCompositionEndingProgress(progress));
    }

    private BaroquenChord GetNextChord(IReadOnlyList<ChordChoice> possibleChordChoices, IReadOnlyList<BaroquenChord> compositionContext)
    {
        var chordChoice = possibleChordChoices.MinBy(static _ => ThreadLocalRandom.Next());

        if (chordChoice is not null)
        {
            return compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice, compositionConfiguration.DefaultNoteTimeSpan);
        }

        logger.NoValidChordChoicesAvailable();

        throw new NoValidChordChoicesAvailableException();
    }

    private List<Measure> ConvertChordsToMeasures(IEnumerable<BaroquenChord> chords)
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

        if (beats.Count > 0)
        {
            measures.Add(new Measure([.. beats], compositionConfiguration.Meter));
        }

        return measures;
    }
}
