using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Infrastructure.Extensions;

/// <summary>
///     A home for extension methods for <see cref="SevenBitNumber"/>.
/// </summary>
public static class SevenBitNumberExtensions
{
    /// <summary>
    ///     Increments the given <see cref="SevenBitNumber"/> by 1.
    /// </summary>
    /// <param name="source">The <see cref="SevenBitNumber"/> to increment.</param>
    /// <param name="value">The value to increment by.</param>
    /// <returns>A new <see cref="SevenBitNumber"/> that is 1 greater than the given <see cref="SevenBitNumber"/>.</returns>
    public static SevenBitNumber Increment(this SevenBitNumber source, byte value = 1) => (SevenBitNumber)(source + value);

    /// <summary>
    ///     Decrements the given <see cref="SevenBitNumber"/> by 1.
    /// </summary>
    /// <param name="source">The <see cref="SevenBitNumber"/> to decrement.</param>
    /// <param name="value">The value to decrement by.</param>
    /// <returns>A new <see cref="SevenBitNumber"/> that is 1 less than the given <see cref="SevenBitNumber"/>.</returns>
    public static SevenBitNumber Decrement(this SevenBitNumber source, byte value = 1) => (SevenBitNumber)(source - value);
}
