using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Collections.Extensions;
using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Logging;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Strategies;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Composers;

/// <inheritdoc cref="IEndingComposer"/>
internal sealed class EndingComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordNumberIdentifier chordNumberIdentifier,
    ICadenceClassifier cadenceClassifier,
    ICadentialTrillApplicator cadentialTrillApplicator,
    IChordSelector chordSelector,
    IDispatcher dispatcher,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : IEndingComposer
{
    private const int PerfectAuthenticCadenceRank = 0;

    private const int ImperfectAuthenticCadenceRank = 1;

    private const int PlainTonicArrivalRank = 2;

    private const int MaxBridgingChords = 25;

    private const int MaxChordsToTonic = 25;

    // While the tonic hunt is still under this many chords, only an authentic (V to I) arrival ends it; past the
    // budget any tonic arrival is accepted again, so the tonic-final guarantee and the MaxChordsToTonic cap are
    // unchanged for compositions that never pass through the dominant.
    private const int AuthenticCadenceChordBudget = 12;

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
                logger.LogBridgingChordComposed(chords.Count, MaxBridgingChords);

                continue;
            }

            logger.LogInfoMessage($"Could not find suitable bridging chord after {MaxBridgingChords} attempts.");

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
                .Skip(1);

            foreach (var measure in ConvertChordsToMeasures(continuationChords))
            {
                composition.Measures.Add(measure);
            }
        }

        DispatchChordsToTonicProgress(MaxChordsToTonic);

        var finalChordOfComposition = composition.Measures[^1].Beats[^1].Chord;

        cadentialTrillApplicator.ApplyTrill(FindPenultimateChord(composition), finalChordOfComposition);

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

    /// <summary>
    ///     Finds the chord sounding immediately before the composition's final chord. By the time the final
    ///     cadence is applied the composition always holds at least two chords across at least two measures
    ///     (the body plus the appended recapitulation), so the lookup only needs to cross one measure boundary.
    /// </summary>
    private static BaroquenChord FindPenultimateChord(Composition composition)
    {
        var lastMeasure = composition.Measures[^1];

        return lastMeasure.Beats.Count >= 2 ? lastMeasure.Beats[^2].Chord : composition.Measures[^2].Beats[^1].Chord;
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
            var requireAuthenticCadence = chords.Count < AuthenticCadenceChordBudget;

            if (TryFindTonicChord(possibleChordChoices, compositionContext, chords, requireAuthenticCadence, cancellationToken))
            {
                break;
            }

            var nextChord = GetNextChord(possibleChordChoices, compositionContext);

            chords.Add(nextChord);
            compositionContext.Add(nextChord);

            if (chords.Count < MaxChordsToTonic)
            {
                logger.LogChordToTonicComposed(chords.Count, MaxChordsToTonic);
                continue;
            }

            logger.LogInfoMessage($"Could not find tonic chord after {MaxChordsToTonic} attempts.");

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
        bool requireAuthenticCadence,
        CancellationToken cancellationToken)
    {
        BaroquenChord? bestTonicChord = null;
        var bestTonicChordRank = int.MaxValue;
        var worstAcceptableRank = requireAuthenticCadence ? ImperfectAuthenticCadenceRank : PlainTonicArrivalRank;

        // Among the reachable tonic voicings the strongest cadence wins: a perfect authentic cadence over an
        // imperfect one over a plain arrival (with plain arrivals only eligible once authenticity is no longer
        // required).
        foreach (var possibleChordChoice in possibleChordChoices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var potentialTonicChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, possibleChordChoice, compositionConfiguration.DefaultNoteTimeSpan);

            if (chordNumberIdentifier.IdentifyChordNumber(potentialTonicChord) != ChordNumber.I)
            {
                continue;
            }

            var cadenceRank = cadenceClassifier.ClassifyCadence(compositionContext[^1], potentialTonicChord) switch
            {
                CadenceType.PerfectAuthentic => PerfectAuthenticCadenceRank,
                CadenceType.ImperfectAuthentic => ImperfectAuthenticCadenceRank,
                _ => PlainTonicArrivalRank
            };

            if (cadenceRank > worstAcceptableRank || cadenceRank >= bestTonicChordRank)
            {
                continue;
            }

            bestTonicChord = potentialTonicChord;
            bestTonicChordRank = cadenceRank;

            if (bestTonicChordRank == PerfectAuthenticCadenceRank)
            {
                break;
            }
        }

        if (bestTonicChord is null)
        {
            return false;
        }

        chords.Add(bestTonicChord);

        return true;
    }

    private void DispatchChordsToTonicProgress(int currentChordCount)
    {
        var progress = (double)currentChordCount / MaxChordsToTonic * 100 / 2 + 50;

        dispatcher.Dispatch(new ProgressCompositionEndingProgress(progress));
    }

    private BaroquenChord GetNextChord(IReadOnlyList<ChordChoice> possibleChordChoices, IReadOnlyList<BaroquenChord> compositionContext)
    {
        var currentChord = compositionContext[^1];

        var candidateChords = possibleChordChoices.Select(chordChoice =>
            currentChord.ApplyChordChoice(compositionConfiguration.Scale, chordChoice, compositionConfiguration.DefaultNoteTimeSpan)
        );

        var nextChord = chordSelector.SelectNextChord(compositionContext, candidateChords);

        if (nextChord is not null)
        {
            return nextChord;
        }

        logger.LogCriticalMessage("No valid chord choices available.");

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
