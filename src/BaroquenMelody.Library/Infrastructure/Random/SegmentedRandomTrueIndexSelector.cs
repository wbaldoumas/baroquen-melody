using System.Collections;

namespace BaroquenMelody.Library.Infrastructure.Random;

/// <inheritdoc cref="IRandomTrueIndexSelector"/>
internal sealed class SegmentedRandomTrueIndexSelector : IRandomTrueIndexSelector
{
    private const int SegmentSize = 1000;

    public int SelectRandomTrueIndex(BitArray bitArray)
    {
        var segmentTrueCounts = new int[(bitArray.Length + SegmentSize - 1) / SegmentSize];

        for (var i = 0; i < bitArray.Length; ++i)
        {
            if (bitArray[i])
            {
                segmentTrueCounts[i / SegmentSize]++;
            }
        }

        var totalTrueCount = segmentTrueCounts.Sum();

        if (totalTrueCount == 0)
        {
            throw new InvalidOperationException("The provided BitArray does not contain any true values.");
        }

        var randomTruePosition = ThreadLocalRandom.Next(totalTrueCount);
        var currentSegment = -1;

        while (randomTruePosition >= 0)
        {
            currentSegment++;
            randomTruePosition -= segmentTrueCounts[currentSegment];
        }

        var startIndex = currentSegment * SegmentSize;
        var endIndex = Math.Min(startIndex + SegmentSize, bitArray.Length);

        for (var i = startIndex; i < endIndex; ++i)
        {
            if (!bitArray[i])
            {
                continue;
            }

            if (++randomTruePosition == 0)
            {
                return i;
            }
        }

        // this can never be reached, but the compiler doesn't know that
        throw new InvalidOperationException("Failed to locate a random true index in the BitArray.");
    }
}
