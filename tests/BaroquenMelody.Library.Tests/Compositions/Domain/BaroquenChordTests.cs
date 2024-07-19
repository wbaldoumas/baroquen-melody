using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
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
        var chord = new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C1)]);

        // act
        var act = () => chord.GetHashCode();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCaseSource(nameof(ContainsVoiceTestCases))]
    public void ContainsVoice_returns_expected_result(BaroquenChord chord, Voice voice, bool expectedContainsVoice)
    {
        // act
        var containsVoice = chord.ContainsVoice(voice);

        // assert
        containsVoice.Should().Be(expectedContainsVoice);
    }

    [Test]
    public void ResetOrnamentation_resets_ornamentation()
    {
        // arrange
        var sopranoC1 = new BaroquenNote(Voice.Soprano, Notes.C1)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.C2) }
        };

        var altoE1 = new BaroquenNote(Voice.Alto, Notes.E1);
        var cMajor = new BaroquenChord([sopranoC1, altoE1]);

        // act
        cMajor.ResetOrnamentation();

        // assert
        cMajor.Notes.Should().BeEquivalentTo(new[]
        {
            new BaroquenNote(Voice.Soprano, Notes.C1),
            new BaroquenNote(Voice.Alto, Notes.E1)
        });
    }

    private static IEnumerable<TestCaseData> ContainsVoiceTestCases
    {
        get
        {
            var sopranoC1 = new BaroquenNote(Voice.Soprano, Notes.C1);
            var altoE1 = new BaroquenNote(Voice.Alto, Notes.E1);

            var cMajor = new BaroquenChord([sopranoC1, altoE1]);

            yield return new TestCaseData(cMajor, Voice.Soprano, true).SetName("Chord contains soprano voice");

            yield return new TestCaseData(cMajor, Voice.Alto, true).SetName("Chord contains alto voice");

            yield return new TestCaseData(cMajor, Voice.Tenor, false).SetName("Chord does not contain tenor voice");
        }
    }

    private static IEnumerable<TestCaseData> EqualityTestCases
    {
        get
        {
            var sopranoC1 = new BaroquenNote(Voice.Soprano, Notes.C1);
            var altoE1 = new BaroquenNote(Voice.Alto, Notes.E1);

            var cMajor = new BaroquenChord([sopranoC1, altoE1]);
            var identicalCMajor = new BaroquenChord([sopranoC1, altoE1]);

            var sopranoD1 = new BaroquenNote(Voice.Soprano, Notes.D1);
            var altoF1 = new BaroquenNote(Voice.Alto, Notes.F1);

            var dMinor = new BaroquenChord([sopranoD1, altoF1]);

            var sopranoC1WithOrnamentation = new BaroquenNote(Voice.Soprano, Notes.C1)
            {
                Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.C2) }
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
