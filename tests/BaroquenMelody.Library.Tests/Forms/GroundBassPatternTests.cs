using BaroquenMelody.Library.Forms;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Forms;

[TestFixture]
internal sealed class GroundBassPatternTests
{
    [Test]
    public void Bank_ContainsDistinctlyIdentifiedPatterns()
    {
        GroundBassPattern.Bank.Should().NotBeEmpty();
        GroundBassPattern.Bank.Select(static pattern => pattern.Identifier).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Bank_PatternsAllStartOnTheTonicAndEndOnTheDominant()
    {
        foreach (var pattern in GroundBassPattern.Bank)
        {
            pattern.ScaleStepOffsets[0].Should().Be(0, "every ground announces itself from the tonic");
            pattern.ScaleStepOffsets[^1].Should().Be(-3, "every ground closes on the dominant a fourth below, so each statement seam forms a dominant-to-tonic pair");
        }
    }

    [Test]
    public void Bank_PatternsAllHaveEvenNoteCounts()
    {
        foreach (var pattern in GroundBassPattern.Bank)
        {
            (pattern.ScaleStepOffsets.Count % 2).Should().Be(0, "at two slots per ground note, an even count fills whole four-slot measures");
        }
    }

    [Test]
    public void Bank_PatternOffsetsStayWithinOneOctaveBelowTheAnchor()
    {
        foreach (var pattern in GroundBassPattern.Bank)
        {
            pattern.ScaleStepOffsets.Should().OnlyContain(static offset => offset <= 0 && offset >= -7, "the anchor is the pattern's highest note and the span must fit a minimum-size bass range");
        }
    }
}
