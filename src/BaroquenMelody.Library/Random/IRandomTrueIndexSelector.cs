using System.Collections;

namespace BaroquenMelody.Library.Random;

/// <summary>
///     A utility for selecting a random true index from a <see cref="BitArray"/>.
/// </summary>
public interface IRandomTrueIndexSelector
{
    /// <summary>
    ///    Selects a random true index from the given <see cref="BitArray"/>.
    /// </summary>
    /// <param name="bitArray">The <see cref="BitArray"/> from which to select a random true index.</param>
    /// <returns>The random true index.</returns>
    int SelectRandomTrueIndex(BitArray bitArray);
}
