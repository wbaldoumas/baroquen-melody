using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition;

/// <summary>
///     Represents a note in a composition.
/// </summary>
/// <param name="Pitch"> The pitch of the note. </param>
/// <param name="Voice"> The voice of the note. </param>
/// <param name="NoteContext"> The previous note context from which this note was generated. </param>
/// <param name="NoteChoice"> The note choice which was used to generate this note. </param>
internal sealed record Note(
    byte Pitch,
    Voice Voice,
    NoteContext NoteContext,
    NoteChoice NoteChoice
);
