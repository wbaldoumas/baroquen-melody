using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Interval = BaroquenMelody.Library.Compositions.Enums.Interval;

namespace BaroquenMelody.Library.Tests.Compositions.Enums.Extensions;

[TestFixture]
internal sealed class IntervalExtensionsTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void FromNotes_generates_expected_interval(BaroquenNote note, BaroquenNote otherNote, Interval expectedInterval) =>
        IntervalExtensions.FromNotes(note, otherNote).Should().Be(expectedInterval);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.B3),
                Interval.MajorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.C4),
                Interval.Unison
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.CSharp4),
                Interval.MinorSecond
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.D4),
                Interval.MajorSecond
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.DSharp4),
                Interval.MinorThird
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.E4),
                Interval.MajorThird
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.F4),
                Interval.PerfectFourth
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.FSharp4),
                Interval.Tritone
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.G4),
                Interval.PerfectFifth
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.GSharp4),
                Interval.MinorSixth
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.A4),
                Interval.MajorSixth
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.ASharp4),
                Interval.MinorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.B4),
                Interval.MajorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.C5),
                Interval.Unison
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.CSharp5),
                Interval.MinorSecond
            );
        }
    }
}
