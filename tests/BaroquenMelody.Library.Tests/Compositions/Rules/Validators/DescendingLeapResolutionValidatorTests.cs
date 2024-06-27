using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules.Validators;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules.Validators;

[TestFixture]
internal sealed class DescendingLeapResolutionValidatorTests
{
    private DescendingLeapResolutionValidator _descendingLeapResolutionValidator = default!;

    [SetUp]
    public void SetUp() => _descendingLeapResolutionValidator = new DescendingLeapResolutionValidator();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void HasInvalidLeapResolution_returns_expected_result(BaroquenChord nextToLastChord, BaroquenChord lastChord, BaroquenChord nextChord, bool expected)
    {
        // act
        var hasInvalidLeapResolution = _descendingLeapResolutionValidator.HasValidLeapResolution(nextToLastChord, lastChord, nextChord, Scale.Parse("C Major").GetNotes().ToList());

        // assert
        hasInvalidLeapResolution.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC2 = new BaroquenNote(Voice.Soprano, Notes.C2);
            var altoE1 = new BaroquenNote(Voice.Alto, Notes.E1);

            var cMajor = new BaroquenChord([sopranoC2, altoE1]);

            var sopranoB1 = new BaroquenNote(Voice.Soprano, Notes.B1);
            var altoD1 = new BaroquenNote(Voice.Alto, Notes.D1);

            var bMinor = new BaroquenChord([sopranoB1, altoD1]);

            var sopranoG1 = new BaroquenNote(Voice.Soprano, Notes.G1);
            var altoC1 = new BaroquenNote(Voice.Alto, Notes.C1);

            var gMajor = new BaroquenChord([sopranoG1, altoC1]);

            var sopranoF1 = new BaroquenNote(Voice.Soprano, Notes.F1);
            var altoA1 = new BaroquenNote(Voice.Alto, Notes.A1);

            var fMajor = new BaroquenChord([sopranoF1, altoA1]);

            yield return new TestCaseData(cMajor, bMinor, gMajor, true).SetName("No need to handle descending leap when it's not a descending leap.");

            yield return new TestCaseData(cMajor, gMajor, fMajor, false).SetName("Incorrectly handles descending leap.");

            yield return new TestCaseData(gMajor, fMajor, cMajor, true).SetName("Correctly handles descending leap.");
        }
    }
}
