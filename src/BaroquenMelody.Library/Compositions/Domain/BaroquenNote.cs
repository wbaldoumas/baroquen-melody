using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a note in a composition.
/// </summary>
/// <param name="Voice">The voice that the note is played in.</param>
/// <param name="Raw">The raw note that is played.</param>
internal sealed record BaroquenNote(Voice Voice, Note Raw);
