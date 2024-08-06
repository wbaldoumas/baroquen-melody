using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Exceptions;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Strategies;

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
        var nextChordVoices = nextChord.Notes.Select(note => note.Voice).ToList();

        return GetPossibleChords(precedingChords)
            .Where(possibleChord => nextChordVoices.TrueForAll(voice => possibleChord[voice].Raw == nextChord[voice].Raw))
            .ToList();
    }

    public BaroquenChord GenerateInitialChord()
    {
        var startingNoteCounts = compositionConfiguration.Scale.I.ToDictionary(noteName => noteName, _ => 0);
        var rawNotes = compositionConfiguration.Scale.GetNotes();

        var notes = compositionConfiguration.VoiceConfigurations
            .Select(voiceConfiguration => new BaroquenNote(voiceConfiguration.Voice, ChooseStartingNote(voiceConfiguration, rawNotes, compositionConfiguration.Scale.I, ref startingNoteCounts)))
            .ToList();

        return new BaroquenChord(notes);
    }

    private List<BaroquenChord> GetPossibleChords(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var currentChord = precedingChords[^1];
        var possibleChordChoices = GetPossibleChordChoices(precedingChords);

        return possibleChordChoices
            .Select(chordChoice => currentChord.ApplyChordChoice(compositionConfiguration.Scale, chordChoice))
            .ToList();
    }

    private Note ChooseStartingNote(
        VoiceConfiguration voiceConfiguration,
        IReadOnlyCollection<Note> notes,
        HashSet<NoteName> startingNoteNames,
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
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && unChosenNotes.Contains(note.NoteName))
                .MinBy(static _ => ThreadLocalRandom.Next()) ?? notes
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && startingNoteNames.Contains(note.NoteName))
                .MinBy(static _ => ThreadLocalRandom.Next());

            if (chosenNote is not null)
            {
                continue;
            }

            logger.CouldNotFindStartingNoteForVoice(voiceConfiguration.Voice);

            throw new CouldNotFindStartingNoteForVoiceException(voiceConfiguration.Voice);
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

    private IEnumerable<(ChordChoice ChordChoice, BaroquenChord Chord)> GetValidChordChoicesAndChords(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var currentChord = precedingChords[^1];

        for (var chordChoiceId = 0; chordChoiceId < chordChoiceRepository.Count; ++chordChoiceId)
        {
            var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceId);
            var nextChord = currentChord.ApplyChordChoice(compositionConfiguration.Scale, chordChoice);

            if (compositionRule.Evaluate(precedingChords, nextChord))
            {
                yield return (chordChoice, nextChord);
            }
        }
    }
}
