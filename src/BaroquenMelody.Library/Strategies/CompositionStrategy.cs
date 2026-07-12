using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Logging;
using BaroquenMelody.Library.Rules;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceEnumerator chordChoiceEnumerator,
    ICompositionRule compositionRule,
    ILogger logger,
    CompositionConfiguration compositionConfiguration,
    IRandomProvider randomProvider,
    int maxRepeatedNotes = 2,
    int maxLookAheadDepth = 2,
    int minLookAheadChordChoices = 2,
    int maxInitialChordAttempts = 50
) : ICompositionStrategy
{
    public IReadOnlyList<ChordChoice> GetPossibleChordChoices(IReadOnlyList<BaroquenChord> precedingChords) => GetValidChordChoicesAndChords(precedingChords)
        .Where(chordChoiceAndChord => HasSubsequentChordChoices([.. precedingChords, chordChoiceAndChord.Chord]))
        .Select(static chordChoiceAndChord => chordChoiceAndChord.ChordChoice)
        .ToList();

    public IReadOnlyList<BaroquenChord> GetPossibleChordsForPartiallyVoicedChords(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var nextChordInstruments = nextChord.Notes.Select(note => note.Instrument).ToList();

        return GetPossibleChords(precedingChords)
            .Where(possibleChord => nextChordInstruments.TrueForAll(instrument => possibleChord[instrument].Raw == nextChord[instrument].Raw))
            .ToList();
    }

    public bool HasPossibleChordForPartiallyVoicedChord(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        EnumerateRuleValidChordsForPartiallyVoicedChord(precedingChords, nextChord).Any();

    public IReadOnlyList<BaroquenChord> GetRuleValidChordsForPartiallyVoicedChord(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        EnumerateRuleValidChordsForPartiallyVoicedChord(precedingChords, nextChord).ToList();

    public BaroquenChord GenerateInitialChord()
    {
        var attempt = 0;
        BaroquenChord initialChord;

        // The initial voicing is not produced by the rule-checked search, so validate each candidate against the
        // composition rules with an empty preceding-chord context and retry a bounded number of times. On exhaustion,
        // fall back to the last candidate, which matches the legacy unvalidated behavior.
        do
        {
            initialChord = GenerateInitialChordCandidate();

            if (compositionRule.Evaluate([], initialChord))
            {
                return initialChord;
            }
        }
        while (++attempt < maxInitialChordAttempts);

        logger.LogNoRuleCompliantInitialChord(maxInitialChordAttempts);

        return initialChord;
    }

    public IEnumerable<BaroquenChord> GetPossibleChords(IReadOnlyList<BaroquenChord> precedingChords) =>
        GetValidChordChoicesAndChords(precedingChords)
            .Where(chordChoiceAndChord => HasSubsequentChordChoices([.. precedingChords, chordChoiceAndChord.Chord]))
            .Select(static chordChoiceAndChord => chordChoiceAndChord.Chord);

    public IEnumerable<(ChordChoice ChordChoice, BaroquenChord Chord)> GetValidChordChoicesAndChords(IReadOnlyList<BaroquenChord> precedingChords)
    {
        foreach (var candidate in chordChoiceEnumerator.EnumerateCandidates(precedingChords[^1]))
        {
            if (compositionRule.Evaluate(precedingChords, candidate.Chord))
            {
                yield return candidate;
            }
        }
    }

    private IEnumerable<BaroquenChord> EnumerateRuleValidChordsForPartiallyVoicedChord(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var nextChordInstruments = nextChord.Notes.Select(static note => note.Instrument).ToList();

        return GetValidChordChoicesAndChords(precedingChords)
            .Select(static chordChoiceAndChord => chordChoiceAndChord.Chord)
            .Where(chord => nextChordInstruments.TrueForAll(instrument => chord[instrument].Raw == nextChord[instrument].Raw));
    }

    private BaroquenChord GenerateInitialChordCandidate()
    {
        var startingNoteCounts = compositionConfiguration.Scale.I.ToDictionary(noteName => noteName, _ => 0);
        var rawNotes = compositionConfiguration.Scale.GetNotes();

        var notes = compositionConfiguration.InstrumentConfigurations
            .Select(instrumentConfiguration =>
                new BaroquenNote(
                    instrumentConfiguration.Instrument,
                    ChooseStartingNote(instrumentConfiguration, rawNotes, compositionConfiguration.Scale.I, ref startingNoteCounts),
                    compositionConfiguration.DefaultNoteTimeSpan
                )
            )
            .ToList();

        return new BaroquenChord(notes);
    }

    private Note ChooseStartingNote(
        InstrumentConfiguration instrumentConfiguration,
        IReadOnlyCollection<Note> notes,
        FrozenSet<NoteName> startingNoteNames,
        ref Dictionary<NoteName, int> startingNoteCounts)
    {
        Note? chosenNote;

        do
        {
            // try to first choose a note that hasn't been chosen at all, then only choose a note that has not already been doubled...
            var unChosenNotes = startingNoteCounts.Where(static startingNoteCount => startingNoteCount.Value == 0)
                .Select(static startingNoteCount => startingNoteCount.Key)
                .ToHashSet();

            chosenNote = notes
                .Where(note => compositionConfiguration.IsNoteInInstrumentRange(instrumentConfiguration.Instrument, note) && unChosenNotes.Contains(note.NoteName))
                .MinByRandom(randomProvider) ?? notes
                .Where(note => compositionConfiguration.IsNoteInInstrumentRange(instrumentConfiguration.Instrument, note) && startingNoteNames.Contains(note.NoteName))
                .MinByRandom(randomProvider);

            if (chosenNote is not null)
            {
                continue;
            }

            logger.LogCouldNotFindStartingNote(instrumentConfiguration.Instrument);

            throw new CouldNotFindStartingNoteForInstrumentException(instrumentConfiguration.Instrument);
        }
        while (startingNoteCounts.TryGetValue(chosenNote.NoteName, out var count) && count >= maxRepeatedNotes);

        startingNoteCounts[chosenNote.NoteName]++;

        return chosenNote;
    }

    private bool HasSubsequentChordChoices(IReadOnlyList<BaroquenChord> precedingChords, int depth = 0)
    {
        if (depth >= maxLookAheadDepth)
        {
            return true;
        }

        var possibleChordChoicesCount = 0;

        return GetValidChordChoicesAndChords(precedingChords)
            .Any(chordChoiceAndChord => HasSubsequentChordChoices([.. precedingChords, chordChoiceAndChord.Chord], depth + 1) && ++possibleChordChoicesCount >= minLookAheadChordChoices);
    }
}
