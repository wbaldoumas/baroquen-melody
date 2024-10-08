﻿using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Interval = BaroquenMelody.Library.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Tests.MusicTheory.Enums.Extensions;

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
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Half),
                Interval.MajorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                Interval.Unison
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half),
                Interval.MinorSecond
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                Interval.MajorSecond
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.DSharp4, MusicalTimeSpan.Half),
                Interval.MinorThird
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                Interval.MajorThird
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
                Interval.PerfectFourth
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.FSharp4, MusicalTimeSpan.Half),
                Interval.Tritone
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                Interval.PerfectFifth
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.GSharp4, MusicalTimeSpan.Half),
                Interval.MinorSixth
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                Interval.MajorSixth
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.ASharp4, MusicalTimeSpan.Half),
                Interval.MinorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half),
                Interval.MajorSeventh
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                Interval.Unison
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.CSharp5, MusicalTimeSpan.Half),
                Interval.MinorSecond
            );
        }
    }
}
