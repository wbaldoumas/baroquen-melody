using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Contexts;

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
    BigInteger GetChordContextId(ChordContext chordContext);

    /// <summary>
    ///    Get the <see cref="ChordContext"/> with the given ID.
    /// </summary>
    /// <param name="id"> The ID of the <see cref="ChordContext"/> to get. </param>
    /// <returns> The <see cref="ChordContext"/> with the given ID. </returns>
    ChordContext GetChordContext(BigInteger id);
}
