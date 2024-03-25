using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.MusicTheory;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceRepository chordChoiceRepository,
    ICompositionRule compositionRule,
    CompositionConfiguration compositionConfiguration
) : ICompositionStrategy
{
    private const int MaxRepeatedNotes = 2;

    /// <summary>
    ///     Hardcoded for now, but this might be configurable in the future and also encompass multiple different chords.
    /// </summary>
    private readonly HashSet<NoteName> validStartingNoteNames =
    [
        compositionConfiguration.Scale.GetDegree(ScaleDegree.Tonic),
        compositionConfiguration.Scale.GetDegree(ScaleDegree.Mediant),
        compositionConfiguration.Scale.GetDegree(ScaleDegree.Dominant)
    ];

    public IReadOnlyList<ChordChoice> GetPossibleChordChoices(IReadOnlyList<Chord> precedingChords)
    {
        var currentChord = precedingChords[^1];
        var result = new List<ChordChoice>();

        for (var chordChoiceId = 0; chordChoiceId < chordChoiceRepository.Count; ++chordChoiceId)
        {
            var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceId);
            var nextChord = currentChord.ApplyChordChoice(compositionConfiguration.Scale, chordChoice);

            if (compositionRule.Evaluate(precedingChords, nextChord))
            {
                result.Add(chordChoice);
            }
        }

        return result;
    }

    public Chord GenerateInitialChord()
    {
        var startingNoteCounts = validStartingNoteNames.ToDictionary(noteName => noteName, _ => 0);
        var rawNotes = compositionConfiguration.Scale.GetNotes().ToList();
        var notes = new HashSet<Note>();

        foreach (var voiceConfiguration in compositionConfiguration.VoiceConfigurations)
        {
            var rawNote = ChooseStartingNote(compositionConfiguration, voiceConfiguration, rawNotes, validStartingNoteNames, ref startingNoteCounts);
            var contextualizedNote = new Note(voiceConfiguration.Voice, rawNote);

            notes.Add(contextualizedNote);
        }

        return new Chord(notes);
    }

    /// <summary>
    ///    Choose a starting note for the given voice, ensuring that the note is within the voice's range and not repeated more than twice.
    /// </summary>
    /// <param name="compositionConfiguration"> The composition configuration. </param>
    /// <param name="voiceConfiguration"> The voice configuration. </param>
    /// <param name="notes"> The notes to choose from. </param>
    /// <param name="startingNoteNames"> The valid starting note names. </param>
    /// <param name="startingNoteCounts"> The counts of starting notes already chosen. </param>
    /// <returns> The chosen starting note. </returns>
    private static Melanchall.DryWetMidi.MusicTheory.Note ChooseStartingNote(
        CompositionConfiguration compositionConfiguration,
        VoiceConfiguration voiceConfiguration,
        IReadOnlyCollection<Melanchall.DryWetMidi.MusicTheory.Note> notes,
        HashSet<NoteName> startingNoteNames,
        ref Dictionary<NoteName, int> startingNoteCounts)
    {
        Melanchall.DryWetMidi.MusicTheory.Note? chosenNote;

        do
        {
            // try to first choose a note that hasn't been chosen at all, then only choose a note that has not already been doubled...
            var unChosenNotes = startingNoteCounts.Where(startingNoteCount => startingNoteCount.Value == 0)
                .Select(startingNoteCount => startingNoteCount.Key)
                .ToHashSet();

            chosenNote = notes
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && unChosenNotes.Contains(note.NoteName))
                .MinBy(_ => ThreadLocalRandom.Next(int.MaxValue)) ?? notes
                .Where(note => compositionConfiguration.IsNoteInVoiceRange(voiceConfiguration.Voice, note) && startingNoteNames.Contains(note.NoteName))
                .OrderBy(_ => ThreadLocalRandom.Next(int.MaxValue))
                .First();
        }
        while (startingNoteCounts.TryGetValue(chosenNote.NoteName, out var count) && count >= MaxRepeatedNotes);

        startingNoteCounts[chosenNote.NoteName]++;

        return chosenNote;
    }
}
