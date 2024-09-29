using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Rules;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceRepository chordChoiceRepository,
    ICompositionRule compositionRule,
    ILogger logger,
    CompositionConfiguration compositionConfiguration,
    int maxRepeatedNotes = 2,
    int maxLookAheadDepth = 2,
    int minLookAheadChordChoices = 2
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

    public BaroquenChord GenerateInitialChord()
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

    public IEnumerable<BaroquenChord> GetPossibleChords(IReadOnlyList<BaroquenChord> precedingChords) =>
        GetValidChordChoicesAndChords(precedingChords)
            .Where(chordChoiceAndChord => HasSubsequentChordChoices([.. precedingChords, chordChoiceAndChord.Chord]))
            .Select(static chordChoiceAndChord => chordChoiceAndChord.Chord);

    public IEnumerable<(ChordChoice ChordChoice, BaroquenChord Chord)> GetValidChordChoicesAndChords(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var currentChord = precedingChords[^1];

        for (var chordChoiceId = 0; chordChoiceId < chordChoiceRepository.Count; ++chordChoiceId)
        {
            var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceId);
            var nextChord = currentChord.ApplyChordChoice(compositionConfiguration.Scale, chordChoice, compositionConfiguration.DefaultNoteTimeSpan);

            if (compositionRule.Evaluate(precedingChords, nextChord))
            {
                yield return (chordChoice, nextChord);
            }
        }
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
                .MinBy(static _ => ThreadLocalRandom.Next()) ?? notes
                .Where(note => compositionConfiguration.IsNoteInInstrumentRange(instrumentConfiguration.Instrument, note) && startingNoteNames.Contains(note.NoteName))
                .MinBy(static _ => ThreadLocalRandom.Next());

            if (chosenNote is not null)
            {
                continue;
            }

            logger.LogCriticalMessage($"Could not find starting note for instrument {instrumentConfiguration.Instrument}.");

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
