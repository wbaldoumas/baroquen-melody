using BaroquenMelody.Library.Compositions.MusicTheory.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.MusicTheory.Extensions;

[TestFixture]
internal sealed class MusicalTimeSpanExtensionsTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void DivideBy_returns_expected_result(MusicalTimeSpan dividend, MusicalTimeSpan divisor, int expected)
    {
        // act
        var result = dividend.DivideBy(divisor);
        var otherResult = dividend.Divide(divisor);

        // assert
        result.Should().Be(expected);
        otherResult.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(MusicalTimeSpan.Whole, MusicalTimeSpan.Whole, 1);

            yield return new TestCaseData(MusicalTimeSpan.Whole, MusicalTimeSpan.Half, 2);

            yield return new TestCaseData(MusicalTimeSpan.Whole, MusicalTimeSpan.Quarter, 4);

            yield return new TestCaseData(MusicalTimeSpan.Half, MusicalTimeSpan.Sixteenth, 8);

            yield return new TestCaseData(MusicalTimeSpan.Quarter.Dotted(1), MusicalTimeSpan.Sixteenth, 6);

            yield return new TestCaseData(MusicalTimeSpan.Half + MusicalTimeSpan.Eighth, MusicalTimeSpan.Sixteenth, 10);

            yield return new TestCaseData(MusicalTimeSpan.Sixteenth, MusicalTimeSpan.ThirtySecond, 2);
        }
    }
}
