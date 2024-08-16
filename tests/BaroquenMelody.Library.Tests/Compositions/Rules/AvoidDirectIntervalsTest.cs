using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Interval = BaroquenMelody.Library.Compositions.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidDirectIntervalsTest
{
    private AvoidDirectIntervals _avoidDirectIntervals = null!;

    [SetUp]
    public void SetUp() => _avoidDirectIntervals = new AvoidDirectIntervals(Interval.PerfectFifth);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        // act
        var result = _avoidDirectIntervals.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);

            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);
            var altoA3 = new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half);
            var altoA2 = new BaroquenNote(Instrument.Two, Notes.A2, MusicalTimeSpan.Half);

            yield return new TestCaseData(
                new List<BaroquenChord>(),
                new BaroquenChord([sopranoC4, altoF3]),
                true
            ).SetName("If it's the first chord, no direct interval.");

            yield return new TestCaseData(
                new List<BaroquenChord> { new([sopranoC4, altoF3]) },
                new BaroquenChord([sopranoD4, altoA2]),
                true
            ).SetName("No direct interval if moving in different directions.");

            yield return new TestCaseData(
                new List<BaroquenChord> { new([sopranoC4, altoF3]) },
                new BaroquenChord([sopranoC4, altoF3]),
                true
            ).SetName("No direct interval if repeated.");

            yield return new TestCaseData(
                new List<BaroquenChord> { new([sopranoD4, altoF3]) },
                new BaroquenChord([sopranoC4, altoA2]),
                true
            ).SetName("No direct interval moving to a different interval than specified.");

            yield return new TestCaseData(
                new List<BaroquenChord> { new([sopranoC4, altoF3]) },
                new BaroquenChord([sopranoD4, altoA3]),
                false
            ).SetName("Direct interval if moving in the same direction to the interval.");
        }
    }
}
