using BaroquenMelody.Library.Compositions.Choices;
using Melanchall.DryWetMidi.MusicTheory;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="Domain.Chord"/>.
/// </summary>
internal static class BaroquenChordExtensions
{
    public static Chord ApplyChordChoice(this Chord chord, Scale scale, ChordChoice chordChoice) => new(
        from noteChoice in chordChoice.NoteChoices
        let voice = noteChoice.Voice
        let note = chord[voice]
        select note.ApplyNoteChoice(scale, noteChoice)
    );
}
