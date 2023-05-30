using System.Collections;
using System.IO.Compression;

namespace BaroquenMelody.Library.Compression;

/// <inheritdoc cref="IBitArrayCompressor"/>
internal sealed class BrotliBitArrayCompressor : IBitArrayCompressor
{
    private readonly CompressionLevel _compressionLevel;

    public BrotliBitArrayCompressor(CompressionLevel compressionLevel) => _compressionLevel = compressionLevel;

    public byte[] Compress(BitArray bits)
    {
        var bytes = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(bytes, 0);

        using var originalStream = new MemoryStream(bytes);
        using var compressedStream = new MemoryStream();

        using (var compressionStream = new BrotliStream(compressedStream, _compressionLevel))
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
