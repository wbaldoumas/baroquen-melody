using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Domain;

[TestFixture]
internal sealed class BaroquenChordTests
{
    [Test]
    [TestCaseSource(nameof(EqualityTestCases))]
    public void Equality_methods_return_expected_results(BaroquenChord? chord, BaroquenChord? otherChord, bool expectedEqualityResult)
    {
        // act + assert
        chord?.Equals(otherChord).Should().Be(expectedEqualityResult);
        chord?.Equals((object?)otherChord).Should().Be(expectedEqualityResult);
        (chord == otherChord).Should().Be(expectedEqualityResult);
        (chord != otherChord).Should().Be(!expectedEqualityResult);
    }

    [Test]
    public void GetHashCode_throws_InvalidOperationException()
    {
        // arrange
        var chord = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)]);

        // act
        var act = () => chord.GetHashCode();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCaseSource(nameof(ContainsInstrumentTestCases))]
    public void ContainsInstrument_returns_expected_result(BaroquenChord chord, Instrument instrument, bool expectedContainsInstrument)
    {
        // act
        var containsInstrument = chord.ContainsInstrument(instrument);

        // assert
        containsInstrument.Should().Be(expectedContainsInstrument);
    }

    [Test]
    public void ResetOrnamentation_resets_ornamentation()
    {
        // arrange
        var sopranoC1 = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.One, Notes.C2, MusicalTimeSpan.Half) }
        };

        var altoE1 = new BaroquenNote(Instrument.Two, Notes.E1, MusicalTimeSpan.Half);
        var cMajor = new BaroquenChord([sopranoC1, altoE1]);

        // act
        cMajor.ResetOrnamentation(MusicalTimeSpan.Half);

        // assert
        cMajor.Notes.Should().BeEquivalentTo(new[]
        {
            new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E1, MusicalTimeSpan.Half)
        });
    }

    private static IEnumerable<TestCaseData> ContainsInstrumentTestCases
    {
        get
        {
            var sopranoC1 = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half);
            var altoE1 = new BaroquenNote(Instrument.Two, Notes.E1, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC1, altoE1]);

            yield return new TestCaseData(cMajor, Instrument.One, true).SetName("Chord contains soprano instrument");

            yield return new TestCaseData(cMajor, Instrument.Two, true).SetName("Chord contains alto instrument");

            yield return new TestCaseData(cMajor, Instrument.Three, false).SetName("Chord does not contain tenor instrument");
        }
    }

    private static IEnumerable<TestCaseData> EqualityTestCases
    {
        get
        {
            var sopranoC1 = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half);
            var altoE1 = new BaroquenNote(Instrument.Two, Notes.E1, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC1, altoE1]);
            var identicalCMajor = new BaroquenChord([sopranoC1, altoE1]);

            var sopranoD1 = new BaroquenNote(Instrument.One, Notes.D1, MusicalTimeSpan.Half);
            var altoF1 = new BaroquenNote(Instrument.Two, Notes.F1, MusicalTimeSpan.Half);

            var dMinor = new BaroquenChord([sopranoD1, altoF1]);

            var sopranoC1WithOrnamentation = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)
            {
                Ornamentations = { new BaroquenNote(Instrument.One, Notes.C2, MusicalTimeSpan.Half) }
            };

            var cMajorWithOrnamentation = new BaroquenChord([sopranoC1WithOrnamentation, altoE1]);

            BaroquenChord? nullChord = null;

            yield return new TestCaseData(cMajor, cMajor, true).SetName("Same chord instances are equal");

            yield return new TestCaseData(cMajor, identicalCMajor, true).SetName("Identical chords are equal");

            yield return new TestCaseData(cMajor, dMinor, false).SetName("Chords with different notes are not equal");

            yield return new TestCaseData(cMajor, cMajorWithOrnamentation, false).SetName("Chords with different ornamentations are not equal");

            yield return new TestCaseData(cMajor, nullChord, false).SetName("Null chord is not equal to any chord");

            yield return new TestCaseData(nullChord, cMajor, false).SetName("Null chord is not equal to any chord");
        }
    }
}
