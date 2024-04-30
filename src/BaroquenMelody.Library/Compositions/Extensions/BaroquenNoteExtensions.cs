using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenNote"/>.
/// </summary>
internal static class BaroquenNoteExtensions
{
    private const int PerfectFourth = 5;

    private const int PerfectFifth = 7;

    private const int PerfectOctave = 12;

    public static BaroquenNote ApplyNoteChoice(this BaroquenNote note, Scale scale, NoteChoice noteChoice)
    {
        var nextNote = noteChoice.Motion switch
        {
            Enums.NoteMotion.Ascending => scale.GetAscendingNotes(note.Raw).ElementAt(noteChoice.ScaleStepChange),
            Enums.NoteMotion.Descending => scale.GetDescendingNotes(note.Raw).ElementAt(noteChoice.ScaleStepChange),
            Enums.NoteMotion.Oblique => note.Raw,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new BaroquenNote(note.Voice, nextNote);
    }

    public static bool IsDissonantWith(this BaroquenNote note, BaroquenNote otherNote) => note.Raw.IsDissonantWith(otherNote.Raw);

    public static bool IsPerfectFourthWith(this BaroquenNote note, BaroquenNote otherNote) => Math.Abs(note.Raw.NoteNumber - otherNote.Raw.NoteNumber) == PerfectFourth;

    public static bool IsPerfectFifthWith(this BaroquenNote note, BaroquenNote otherNote) => Math.Abs(note.Raw.NoteNumber - otherNote.Raw.NoteNumber) == PerfectFifth;

    public static bool IsPerfectOctaveWith(this BaroquenNote note, BaroquenNote otherNote) => Math.Abs(note.Raw.NoteNumber - otherNote.Raw.NoteNumber) == PerfectOctave;

    public static NoteMotion GetDirectionTo(this BaroquenNote precedingNote, BaroquenNote currentNote)
    {
        if (precedingNote.Raw.NoteNumber < currentNote.Raw.NoteNumber)
        {
            return NoteMotion.Ascending;
        }

        return precedingNote.Raw.NoteNumber > currentNote.Raw.NoteNumber ? NoteMotion.Descending : NoteMotion.Oblique;
    }
}
