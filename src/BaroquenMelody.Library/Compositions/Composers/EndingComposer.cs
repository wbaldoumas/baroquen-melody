﻿using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Collections.Extensions;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IEndingComposer"/>
internal sealed class EndingComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordNumberIdentifier chordNumberIdentifier,
    CompositionConfiguration compositionConfiguration
) : IEndingComposer
{
    public Composition Compose(Composition composition, BaroquenTheme theme)
    {
        var bridgingComposition = GetBridgingComposition(composition, theme);

        composition.Measures[^1].Beats[^1] = bridgingComposition.Measures[0].Beats[0];

        var continuationChords = bridgingComposition.Measures
            .SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
            .TrimEdges()
            .Concat(theme.Recapitulation.SelectMany(measure => measure.Beats.Select(beat => new BaroquenChord(beat.Chord))));

        foreach (var measure in ConvertChordsToMeasures(continuationChords))
        {
            composition.Measures.Add(measure);
        }

        ApplyFinalCadence(composition);

        return composition;
    }

    private Composition GetBridgingComposition(Composition composition, BaroquenTheme theme)
    {
        var firstChordOfRecapitulation = new BaroquenChord(theme.Recapitulation[0].Beats[0].Chord);

        firstChordOfRecapitulation.ResetOrnamentation();

        var bridgingChords = GetBridgingChords(composition, firstChordOfRecapitulation);
        var lastChordOfComposition = new BaroquenChord(composition.Measures[^1].Beats[^1].Chord);

        lastChordOfComposition.ResetOrnamentation();

        var chords = new List<BaroquenChord> { lastChordOfComposition };

        chords.AddRange(bridgingChords);
        chords.Add(firstChordOfRecapitulation);

        var measures = ConvertChordsToMeasures(chords);
        var bridgingComposition = new Composition(measures);

        compositionDecorator.Decorate(bridgingComposition);

        return bridgingComposition;
    }

    private List<BaroquenChord> GetBridgingChords(Composition composition, BaroquenChord firstChordOfRecapitulation)
    {
        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            composition.Measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
        );

        var chords = new List<BaroquenChord>();

        while (true)
        {
            var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords(compositionContext, firstChordOfRecapitulation);

            if (possibleChords.Count > 0)
            {
                break;
            }

            var nextChord = GetBridgingChord(compositionContext, firstChordOfRecapitulation);

            compositionContext.Add(nextChord);
            chords.Add(nextChord);
        }

        return chords;
    }

    private BaroquenChord GetBridgingChord(IReadOnlyList<BaroquenChord> compositionContext, BaroquenChord targetChord)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(compositionContext);

        foreach (var chordChoice in possibleChordChoices)
        {
            var workingCompositionContext = new FixedSizeList<BaroquenChord>(compositionConfiguration.CompositionContextSize);

            foreach (var chord in compositionContext)
            {
                workingCompositionContext.Add(chord);
            }

            var possibleChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);

            workingCompositionContext.Add(possibleChord);

            var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords(workingCompositionContext, targetChord);

            if (possibleChords.Count <= 0)
            {
                continue;
            }

            return possibleChord;
        }

        var fallbackChordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next()).First();

        return compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, fallbackChordChoice);
    }

    private void ApplyFinalCadence(Composition composition)
    {
        if (chordNumberIdentifier.IdentifyChordNumber(composition.Measures[^1].Beats[^1].Chord) != ChordNumber.I)
        {
            var compositionWithTonicFinalChord = GetCompositionWithTonicFinalChord(composition);

            composition.Measures[^1].Beats[^1] = compositionWithTonicFinalChord.Measures[0].Beats[0];

            var continuationChords = compositionWithTonicFinalChord.Measures
                .SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
                .Skip(1)
                .ToList();

            foreach (var measure in ConvertChordsToMeasures(continuationChords))
            {
                composition.Measures.Add(measure);
            }
        }

        var finalChordOfComposition = composition.Measures[^1].Beats[^1].Chord;

        finalChordOfComposition.ResetOrnamentation();

        foreach (var note in finalChordOfComposition.Notes)
        {
            note.Duration *= 2;
        }

        var restingChord = new BaroquenChord(finalChordOfComposition);

        foreach (var note in restingChord.Notes)
        {
            note.OrnamentationType = OrnamentationType.Rest;
        }

        composition.Measures[^1].Beats.Add(new Beat(restingChord));
    }

    private Composition GetCompositionWithTonicFinalChord(Composition composition)
    {
        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            composition.Measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
        );

        var lastChordOfComposition = compositionContext[^1];

        var chords = new List<BaroquenChord> { lastChordOfComposition };

        while (true)
        {
            var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(compositionContext);
            var foundTonicChord = false;

            foreach (var possibleChordChoice in possibleChordChoices)
            {
                var potentialTonicChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, possibleChordChoice);

                if (chordNumberIdentifier.IdentifyChordNumber(potentialTonicChord) != ChordNumber.I)
                {
                    continue;
                }

                chords.Add(potentialTonicChord);
                foundTonicChord = true;

                break;
            }

            if (foundTonicChord)
            {
                break;
            }

            var chordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next()).First();
            var nextChord = compositionContext[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);

            chords.Add(nextChord);
            compositionContext.Add(nextChord);
        }

        var compositionWithTonicFinalChord = new Composition(ConvertChordsToMeasures(chords));

        compositionDecorator.Decorate(compositionWithTonicFinalChord);

        return compositionWithTonicFinalChord;
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