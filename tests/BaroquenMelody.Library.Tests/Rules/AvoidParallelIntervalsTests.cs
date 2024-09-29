using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Interval = BaroquenMelody.Library.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Tests.Rules;

[TestFixture]
internal sealed class AvoidParallelIntervalsTests
{
    private AvoidParallelIntervals _avoidParallelIntervals = null!;

    [SetUp]
    public void SetUp()
    {
        _avoidParallelIntervals = new AvoidParallelIntervals(Interval.PerfectFifth);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        // act
        var result = _avoidParallelIntervals.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half);
            var bassC1 = new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC4, altoE3, tenorG2, bassC1]);

            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);
            var tenorA2 = new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half);
            var bassD1 = new BaroquenNote(Instrument.Four, Notes.D1, MusicalTimeSpan.Half);

            var dMinor = new BaroquenChord([sopranoD4, altoF3, tenorA2, bassD1]);

            var altoA3 = new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half);
            var tenorF2 = new BaroquenNote(Instrument.Three, Notes.F2, MusicalTimeSpan.Half);

            var dMinorDifferentInstruments = new BaroquenChord([sopranoD4, altoA3, tenorF2, bassD1]);

            var tenorA1 = new BaroquenNote(Instrument.Three, Notes.A1, MusicalTimeSpan.Half);

            var dMinorDifferentDirection = new BaroquenChord([sopranoD4, altoF3, tenorA1, bassD1]);

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor, true).SetName("No parallel fifths if it's the first chord");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, dMinor, false).SetName("Parallel fifths present when instruments moving in same direction.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, dMinorDifferentInstruments, true).SetName("No parallel fifths when instruments are different.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, dMinorDifferentDirection, true).SetName("No parallel fifths when instruments are moving in different directions.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, cMajor, true).SetName("No parallel fifths when chords are identical.");
        }
    }
}
