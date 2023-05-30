using BaroquenMelody.Library.Compression;
using FluentAssertions;
using NUnit.Framework;
using System.Collections;
using System.IO.Compression;

namespace BaroquenMelody.Library.Tests.Compression;

[TestFixture]
internal sealed class BrotliBitArrayCompressorTests
{
    private BrotliBitArrayCompressor? _compressor;

    [SetUp]
    public void SetUp() => _compressor = new BrotliBitArrayCompressor(CompressionLevel.Fastest);

    [Test]
    public void CompressDecompress_Should_ReturnOriginalBitArray()
    {
        // arrange
        var bits = Enumerable.Range(0, 500000).Select(i => i % 2 == 0).ToArray();
        var bitArray = new BitArray(bits);

        // act
        var compressed = _compressor!.Compress(bitArray);
        var decompressed = _compressor.Decompress(compressed);

        // assert
        for (var i = 0; i < bits.Length; i++)
        {
            decompressed[i].Should().Be(bits[i]);
        }
    }
}
