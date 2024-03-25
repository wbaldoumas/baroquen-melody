using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenChord"/>.
/// </summary>
internal static class BaroquenChordExtensions
{
    public static BaroquenChord ApplyChordChoice(this BaroquenChord chord, Scale scale, ChordChoice chordChoice) => new(
        from noteChoice in chordChoice.NoteChoices
        let voice = noteChoice.Voice
        let note = chord[voice]
        select note.ApplyNoteChoice(scale, noteChoice)
    );
}
