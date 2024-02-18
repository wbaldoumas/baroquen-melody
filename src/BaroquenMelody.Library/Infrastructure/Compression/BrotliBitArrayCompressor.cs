using System.Collections;
using System.IO.Compression;

namespace BaroquenMelody.Library.Infrastructure.Compression;

/// <inheritdoc cref="IBitArrayCompressor"/>
internal sealed class BrotliBitArrayCompressor(CompressionLevel compressionLevel) : IBitArrayCompressor
{
    public byte[] Compress(BitArray bits)
    {
        var bytes = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(bytes, 0);

        using var originalStream = new MemoryStream(bytes);
        using var compressedStream = new MemoryStream();

        using (var compressionStream = new BrotliStream(compressedStream, compressionLevel))
        {
            originalStream.CopyTo(compressionStream);
        }

        return compressedStream.ToArray();
    }

    public BitArray Decompress(byte[] bytes)
    {
        using var compressedStream = new MemoryStream(bytes);
        using var decompressedStream = new MemoryStream();

        using (var decompressionStream = new BrotliStream(compressedStream, CompressionMode.Decompress))
        {
            decompressionStream.CopyTo(decompressedStream);
        }

        return new BitArray(decompressedStream.ToArray());
    }
}
