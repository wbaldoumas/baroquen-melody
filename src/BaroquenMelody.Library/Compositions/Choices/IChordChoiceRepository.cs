using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <summary>
///    Represents a repository of <see cref="ChordChoice"/>s.
/// </summary>
internal interface IChordChoiceRepository
{
    /// <summary>
    ///   The number of available chord choices.
    /// </summary>
    public BigInteger Count { get; }

    /// <summary>
    ///   Gets the <see cref="ChordChoice"/> at the given index.
    /// </summary>
    /// <param name="index"> The index of the <see cref="ChordChoice"/> to get. </param>
    /// <returns> The <see cref="ChordChoice"/> at the given index. </returns>
    public ChordChoice GetChordChoice(BigInteger index);

    /// <summary>
    ///     Get the ID of the given <see cref="ChordChoice"/>.
    /// </summary>
    /// <param name="chordChoice"> The <see cref="ChordChoice"/> to get the ID of. </param>
    /// <returns> The ID of the given <see cref="ChordChoice"/>. </returns>
    BigInteger GetChordChoiceIndex(ChordChoice chordChoice);
}
