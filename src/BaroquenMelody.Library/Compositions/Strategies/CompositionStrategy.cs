using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Contexts.Extensions;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceRepository chordChoiceRepository,
    IChordContextRepository chordContextRepository,
    IRandomTrueIndexSelector randomTrueIndexSelector,
    IDictionary<BigInteger, BitArray> chordContextToChordChoiceMap,
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

    public void PreLearn()
    {
        Parallel.For(0, (int)chordContextRepository.Count, chordContextId =>
        {
            var chordContext = chordContextRepository.GetChordContext(chordContextId);
            var previousChord = chordContext.ToContextualizedChord();

            for (var chordChoiceId = 0; chordChoiceId < chordChoiceRepository.Count; chordChoiceId++)
            {
                var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceId);
                var nextChord = chordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

                if (!compositionRule.Evaluate(previousChord, nextChord))
                {
                    chordContextToChordChoiceMap[chordContextId][chordChoiceId] = false;
                }
            }
        });
    }

    public ChordChoice GetNextChordChoice(ChordContext chordContext)
    {
        var chordContextId = chordContextRepository.GetChordContextId(chordContext);
        var chordChoiceIndices = chordContextToChordChoiceMap[chordContextId];

        while (true)
        {
            var chordChoiceId = randomTrueIndexSelector.SelectRandomTrueIndex(chordChoiceIndices);
            var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceId);
            var chord = chordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

            if (chord.ContextualizedNotes.All(voicedNote => compositionConfiguration.IsNoteInVoiceRange(voicedNote.Voice, voicedNote.Note)))
            {
                return chordChoice;
            }

            InvalidateChordChoice(chordContext, chordChoice);
        }
    }

    public void InvalidateChordChoice(ChordContext chordContext, ChordChoice chordChoice)
    {
        var chordContextId = chordContextRepository.GetChordContextId(chordContext);
        var chordChoiceId = chordChoiceRepository.GetChordChoiceId(chordChoice);

        chordContextToChordChoiceMap[chordContextId][(int)chordChoiceId] = false;
    }

    /// <summary>
    ///     Select an initial chord to start the composition. For now, just select a root chord based on the scale.
    /// </summary>
    /// <returns> The initial chord. </returns>
    public ContextualizedChord GetInitialChord()
    {
        var startingNoteCounts = validStartingNoteNames.ToDictionary(noteName => noteName, _ => 0);
        var contextualizedNotes = new HashSet<ContextualizedNote>();
        var notes = compositionConfiguration.Scale.GetNotes().ToList();

        foreach (var voiceConfiguration in compositionConfiguration.VoiceConfigurations)
        {
            var note = ChooseStartingNote(compositionConfiguration, voiceConfiguration, notes, validStartingNoteNames, ref startingNoteCounts);

            var contextualizedNote = new ContextualizedNote(
                note,
                voiceConfiguration.Voice,
                new NoteContext(voiceConfiguration.Voice, note, NoteMotion.Oblique, NoteSpan.None),
                new NoteChoice(voiceConfiguration.Voice, NoteMotion.Oblique, 0)
            );

            contextualizedNotes.Add(contextualizedNote);
        }

        return new ContextualizedChord(
            contextualizedNotes,
            new ChordContext(contextualizedNotes.Select(contextualizedNote => contextualizedNote.ArrivedFromNoteContext)),
            new ChordChoice(contextualizedNotes.Select(contextualizedNote => contextualizedNote.ArrivedFromNoteChoice))
        );
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
    private static Note ChooseStartingNote(
        CompositionConfiguration compositionConfiguration,
        VoiceConfiguration voiceConfiguration,
        IReadOnlyCollection<Note> notes,
        HashSet<NoteName> startingNoteNames,
        ref Dictionary<NoteName, int> startingNoteCounts)
    {
        Note? chosenNote;

        do
        {
            // try to first choose a note that hasn't been chosen at all, then only choose a note that has not already been doubled.
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
