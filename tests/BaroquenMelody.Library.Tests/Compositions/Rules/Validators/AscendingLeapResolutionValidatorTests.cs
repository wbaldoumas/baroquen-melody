using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules.Validators;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules.Validators;

[TestFixture]
internal sealed class AscendingLeapResolutionValidatorTests
{
    private AscendingLeapResolutionValidator _ascendingLeapResolutionValidator = default!;

    [SetUp]
    public void SetUp() => _ascendingLeapResolutionValidator = new AscendingLeapResolutionValidator();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void HasInvalidLeapResolution_returns_expected_result(BaroquenChord nextToLastChord, BaroquenChord lastChord, BaroquenChord nextChord, bool expected)
    {
        // act
        var hasValidLeapResolution = _ascendingLeapResolutionValidator.HasValidLeapResolution(nextToLastChord, lastChord, nextChord, Scale.Parse("C Major").GetNotes().ToList());

        // assert
        hasValidLeapResolution.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC1 = new BaroquenNote(Voice.Soprano, Notes.C1);
            var altoE1 = new BaroquenNote(Voice.Alto, Notes.E1);

            var cMajor = new BaroquenChord([sopranoC1, altoE1]);

            var sopranoD1 = new BaroquenNote(Voice.Soprano, Notes.D1);
            var altoF1 = new BaroquenNote(Voice.Alto, Notes.F1);

            var dMinor = new BaroquenChord([sopranoD1, altoF1]);

            var sopranoF1 = new BaroquenNote(Voice.Soprano, Notes.F1);
            var altoA1 = new BaroquenNote(Voice.Alto, Notes.A1);

            var fMajor = new BaroquenChord([sopranoF1, altoA1]);

            var sopranoG1 = new BaroquenNote(Voice.Soprano, Notes.G1);
            var altoB1 = new BaroquenNote(Voice.Alto, Notes.B1);

            var gMajor = new BaroquenChord([sopranoG1, altoB1]);

            yield return new TestCaseData(cMajor, dMinor, fMajor, true).SetName("No need to handle ascending leap when it's not an ascending leap.");

            yield return new TestCaseData(cMajor, fMajor, gMajor, false).SetName("Incorrectly handles ascending leap.");

            yield return new TestCaseData(cMajor, gMajor, fMajor, true).SetName("Correctly handles ascending leap.");
        }
    }
}
