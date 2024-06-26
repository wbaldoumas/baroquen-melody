﻿using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceRepository chordChoiceRepository,
    ICompositionRule compositionRule,
    CompositionConfiguration compositionConfiguration,
    int maxRepeatedNotes = 2,
    int maxLookAheadDepth = 2,
    int minLookAheadChordChoices = 2
) : ICompositionStrategy
{
    /// <summary>
    ///     Hardcoded for now, but this might be configurable in the future and also encompass multiple different chords.
    /// </summary>
    private readonly HashSet<NoteName> validStartingNoteNames =
    [
        compositionConfiguration.Scale.Raw.GetDegree(ScaleDegree.Tonic),
        compositionConfiguration.Scale.Raw.GetDegree(ScaleDegree.Mediant),
        compositionConfiguration.Scale.Raw.GetDegree(ScaleDegree.Dominant)
    ];

    public IReadOnlyList<ChordChoice> GetPossibleChordChoices(IReadOnlyList<BaroquenChord> precedingChords) => GetValidChordChoicesAndChords(precedingChords)
        .Where(chordChoiceAndChord => HasSubsequentChordChoices([.. precedingChords, chordChoiceAndChord.Chord]))
        .Select(chordChoiceAndChord => chordChoiceAndChord.ChordChoice)
        .ToList();

    public BaroquenChord GenerateInitialChord()
    {
        var startingNoteCounts = validStartingNoteNames.ToDictionary(noteName => noteName, _ => 0);
        var rawNotes = compositionConfiguration.Scale.GetNotes().ToList();

        var notes = compositionConfiguration.VoiceConfigurations
            .Select(voiceConfiguration => new BaroquenNote(voiceConfiguration.Voice, ChooseStartingNote(voiceConfiguration, rawNotes, validStartingNoteNames, ref startingNoteCounts)))
            .ToList();

        return new BaroquenChord(notes);
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
            var unChosenNotes = startingNoteCounts.Where(startingNoteCount => startingNoteCount.Value == 0)
                .Select(startingNoteCount => startingNoteCount.Key)
                .ToHashSet();

            chosenNote = notes
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && unChosenNotes.Contains(note.NoteName))
                .MinBy(_ => ThreadLocalRandom.Next()) ?? notes
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && startingNoteNames.Contains(note.NoteName))
                .OrderBy(_ => ThreadLocalRandom.Next())
                .First();
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
