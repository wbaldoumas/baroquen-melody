using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;

namespace BaroquenMelody.Library.Composition;

/// <summary>
///    Represents a chord in a composition.
/// </summary>
/// <param name="Notes"> The notes which make up the chord. </param>
/// <param name="ChordContext"> The previous chord context from which this chord was generated. </param>
/// <param name="ChordChoice"> The chord choice which was used to generate this chord. </param>
internal sealed record Chord(
    ISet<Note> Notes,
    ChordContext ChordContext,
    ChordChoice ChordChoice
);
