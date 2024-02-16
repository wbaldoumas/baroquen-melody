using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <summary>
///     The context of a note in a composition.
/// </summary>
/// <param name="Voice"> The voice associated with the context. </param>
/// <param name="Note"> The note associated with the context. </param>
/// <param name="NoteMotion"> The motion which was used to arrive at the note. </param>
/// <param name="NoteSpan"> The note span which was used to arrive at the note. </param>
internal sealed record NoteContext(
    Voice Voice,
    Note Note,
    NoteMotion NoteMotion,
    NoteSpan NoteSpan
);
