using System.Collections;

namespace BaroquenMelody.Library.Infrastructure.Compression;

/// <summary>
///     A compressor for <see cref="BitArray" />s.
/// </summary>
internal interface IBitArrayCompressor
{
    /// <summary>
    ///     Compresses a <see cref="BitArray" /> into a byte array.
    /// </summary>
    /// <param name="bits">The <see cref="BitArray" /> to compress.</param>
    /// <returns>The compressed <see cref="BitArray" /> as a byte array.</returns>
    byte[] Compress(BitArray bits);

    /// <summary>
    ///     Decompresses a byte array into a <see cref="BitArray" />.
    /// </summary>
    /// <param name="bytes">The byte array to decompress.</param>
    /// <returns>The decompressed byte array as a <see cref="BitArray" />.</returns>
    BitArray Decompress(byte[] bytes);
}
