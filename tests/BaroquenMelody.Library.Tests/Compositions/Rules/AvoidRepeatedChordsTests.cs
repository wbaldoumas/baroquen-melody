using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidRepeatedChordsTests
{
    private AvoidRepeatedChords _avoidRepeatedChords = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        _avoidRepeatedChords = new AvoidRepeatedChords(new ChordNumberIdentifier(compositionConfiguration));
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Validate_returns_expected_result(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedIsValid)
    {
        // act
        var result = _avoidRepeatedChords.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().Be(expectedIsValid);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var sopranoE4 = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
            var sopranoG4 = new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half);
            var sopranoA4 = new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half);

            var altoC3 = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
            var altoG3 = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);

            var tenorC2 = new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half);
            var tenorE2 = new BaroquenNote(Instrument.Three, Notes.E2, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half);

            var cMajor1 = new BaroquenChord([sopranoC4, altoE3, tenorG2]);
            var cMajor2 = new BaroquenChord([sopranoE4, altoG3, tenorC2]);
            var cMajor3 = new BaroquenChord([sopranoG4, altoC3, tenorE2]);

            var aMinor = new BaroquenChord([sopranoA4, altoE3, tenorC2]);

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor1, true).SetName("No repetition without preceding chords.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor1, cMajor2 }, aMinor, true).SetName("No repetition when last chord changes.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor1, aMinor }, cMajor2, true).SetName("No repetition when middle chord changes.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor1, cMajor2 }, cMajor3, false).SetName("Repetition when last chord repeats.");
        }
    }
}
