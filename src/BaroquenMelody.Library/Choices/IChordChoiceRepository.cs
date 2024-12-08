using System.Numerics;

namespace BaroquenMelody.Library.Choices;

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
    ///   Gets the <see cref="ChordChoice"/> for the given ID.
    /// </summary>
    /// <param name="id"> The ID of the <see cref="ChordChoice"/> to get. </param>
    /// <returns> The <see cref="ChordChoice"/> for the given ID. </returns>
    public ChordChoice GetChordChoice(BigInteger id);
}
