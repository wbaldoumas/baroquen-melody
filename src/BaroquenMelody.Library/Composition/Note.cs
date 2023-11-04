using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;

namespace BaroquenMelody.Library.Composition;

/// <summary>
///     Represents a note in a composition.
/// </summary>
/// <param name="Pitch"> The pitch of the note. </param>
/// <param name="NoteContext"> The previous note context from which this note was generated. </param>
/// <param name="NoteChoice"> The note choice which was used to generate this note. </param>
internal sealed record Note(
    byte Pitch,
    NoteContext NoteContext,
    NoteChoice NoteChoice
);
