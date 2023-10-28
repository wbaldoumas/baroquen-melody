using System.Numerics;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <summary>
///     Represents a repository of <see cref="ChordContext"/>s.
/// </summary>
internal interface IChordContextRepository
{
    /// <summary>
    ///   The number of available chord contexts.
    /// </summary>
    public BigInteger Count { get; }

    /// <summary>
    ///     Get the ID of the given <see cref="ChordContext"/>.
    /// </summary>
    /// <param name="chordContext"> The <see cref="ChordContext"/> to get the ID of. </param>
    /// <returns> The ID of the given <see cref="ChordContext"/>. </returns>
    BigInteger GetChordContextIndex(ChordContext chordContext);
}
