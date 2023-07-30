using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <summary>
///     The context of a note in a composition.
/// </summary>
/// <param name="Voice"> The voice associated with the context. </param>
/// <param name="Pitch"> The pitch of the note associated with the context. </param>
/// <param name="NoteMotion"> The motion which was used to arrive at the pitch. </param>
/// <param name="NoteSpan"> The note span which was used to arrive at the pitch. </param>
internal sealed record NoteContext(
    Voice Voice,
    byte Pitch,
    NoteMotion NoteMotion,
    NoteSpan NoteSpan
);
