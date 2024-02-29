using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extensions for the <see cref="Note"/> class.
/// </summary>
internal static class NoteExtensions
{
    private const int HalfStep = 1;

    private const int WholeStep = 2;

    private const int Tritone = 6;

    private const int MinorSeventh = 10;

    private const int MajorSeventh = 11;

    private const int Octave = 12;

    public static bool IsDissonantWith(this Note note, Note otherNote) => Math.Abs(note.NoteNumber % Octave - otherNote.NoteNumber % Octave) switch
    {
        HalfStep => true,
        WholeStep => true,
        Tritone => true,
        MinorSeventh => true,
        MajorSeventh => true,
        _ => false
    };
}
