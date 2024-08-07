﻿using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidRepetitionTests
{
    private AvoidRepetition _avoidRepetition = null!;

    [SetUp]
    public void SetUp() => _avoidRepetition = new AvoidRepetition();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _avoidRepetition.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half);
            var bassC1 = new BaroquenNote(Voice.Bass, Notes.C1, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC4, altoE3, tenorG2, bassC1]);

            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4, MusicalTimeSpan.Half);
            var altoA3 = new BaroquenNote(Voice.Alto, Notes.A3, MusicalTimeSpan.Half);
            var tenorC3 = new BaroquenNote(Voice.Tenor, Notes.C3, MusicalTimeSpan.Half);
            var bassF2 = new BaroquenNote(Voice.Bass, Notes.F2, MusicalTimeSpan.Half);

            var fMajor = new BaroquenChord([sopranoF4, altoA3, tenorC3, bassF2]);

            var sopranoA4 = new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half);
            var altoC4 = new BaroquenNote(Voice.Alto, Notes.C4, MusicalTimeSpan.Half);
            var tenorE3 = new BaroquenNote(Voice.Tenor, Notes.E3, MusicalTimeSpan.Half);
            var bassA2 = new BaroquenNote(Voice.Bass, Notes.A2, MusicalTimeSpan.Half);

            var aMinor = new BaroquenChord([sopranoA4, altoC4, tenorE3, bassA2]);

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor, true).SetName("First chord is not a repetition.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, cMajor, cMajor }, cMajor, false).SetName("Repetition of the same chord is not allowed.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, fMajor, aMinor }, cMajor, true).SetName("Non-repeating chords are not a repetition");
        }
    }
}
