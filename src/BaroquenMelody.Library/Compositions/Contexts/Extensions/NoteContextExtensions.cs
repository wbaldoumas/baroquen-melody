﻿using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Contexts.Extensions;

internal static class NoteContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="NoteChoice"/> to the given <see cref="NoteContext"/> to generate the next note.
    /// </summary>
    /// <param name="noteContext"> The note context. </param>
    /// <param name="noteChoice"> The note choice. </param>
    /// <param name="scale"> The scale to be used in the generation of the next note. </param>
    /// <returns> The next note. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the given <see cref="NoteChoice"/> has an invalid <see cref="NoteMotion"/>. </exception>
    public static ContextualizedNote ApplyNoteChoice(this NoteContext noteContext, NoteChoice noteChoice, Scale scale)
    {
        var note = noteChoice.Motion switch
        {
            NoteMotion.Ascending => scale.GetAscendingNotes(noteContext.Note).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Descending => scale.GetDescendingNotes(noteContext.Note).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Oblique => noteContext.Note,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new ContextualizedNote(
            note,
            noteChoice.Voice,
            noteContext,
            noteChoice
        );
    }

    /// <summary>
    ///     Generates a <see cref="ChordContext"/> from the given tuple of <see cref="NoteContext"/>s.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteContext"/>s to convert. </param>
    /// <returns> The <see cref="ChordContext"/> representing the given tuple of <see cref="NoteContext"/>s. </returns>
    public static ChordContext ToChordContext(this (NoteContext, NoteContext) source) => new([source.Item1, source.Item2]);

    /// <summary>
    ///     Generates a <see cref="ChordContext"/> from the given tuple of <see cref="NoteContext"/>s.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteContext"/>s to convert. </param>
    /// <returns> The <see cref="ChordContext"/> representing the given tuple of <see cref="NoteContext"/>s. </returns>
    public static ChordContext ToChordContext(this (NoteContext, NoteContext, NoteContext) source) => new([source.Item1, source.Item2, source.Item3]);

    /// <summary>
    ///     Generates a <see cref="ChordContext"/> from the given tuple of <see cref="NoteContext"/>s.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteContext"/>s to convert. </param>
    /// <returns> The <see cref="ChordContext"/> representing the given tuple of <see cref="NoteContext"/>s. </returns>
    public static ChordContext ToChordContext(this (NoteContext, NoteContext, NoteContext, NoteContext) source) => new([source.Item1, source.Item2, source.Item3, source.Item4]);
}