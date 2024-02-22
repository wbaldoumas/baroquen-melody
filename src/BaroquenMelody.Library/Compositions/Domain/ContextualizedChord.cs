using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a chord in a composition.
/// </summary>
/// <param name="Notes"> The notes which make up the chord. </param>
/// <param name="ChordContext"> The previous chord context from which this chord was generated. </param>
/// <param name="ChordChoice"> The chord choice which was used to generate this chord. </param>
internal sealed record ContextualizedChord(
    ISet<ContextualizedNote> Notes,
    ChordContext ChordContext,
    ChordChoice ChordChoice
);
