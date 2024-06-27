using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Rules.Validators;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class HandleLeapTests
{
    private HandleLeap _handleLeap = null!;

    private ILeapResolutionValidator _mockLeapResolutionValidator = null!;

    [SetUp]
    public void SetUp()
    {
        var phrasingConfiguration = new PhrasingConfiguration(
            PhraseLengths: [2, 4, 8],
            MaxPhraseRepetitions: 4,
            MinPhraseRepetitionPoolSize: 10,
            PhraseRepetitionProbability: 90
        );

        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.G5, Notes.G6),
                new(Voice.Alto, Notes.C4, Notes.G5),
                new(Voice.Tenor, Notes.C3, Notes.C4),
                new(Voice.Bass, Notes.C2, Notes.C3)
            },
            phrasingConfiguration,
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            100
        );

        _mockLeapResolutionValidator = Substitute.For<ILeapResolutionValidator>();

        _mockLeapResolutionValidator.HasValidLeapResolution(
            Arg.Any<BaroquenChord>(),
            Arg.Any<BaroquenChord>(),
            Arg.Any<BaroquenChord>(),
            Arg.Any<IList<Note>>()
        ).Returns(true);

        _handleLeap = new HandleLeap(compositionConfiguration, _mockLeapResolutionValidator);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult) =>
        _handleLeap.Evaluate(precedingChords, nextChord).Should().Be(expectedResult);

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

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor, true).SetName("No need to handle ascending leap if it's the first chord.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, dMinor, true).SetName("No need to handle ascending leap if it's the second chord.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, gMajor }, cMajor, true).SetName("No need to handle ascending leap when returning to same chord.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, gMajor }, fMajor, true).SetName("Correctly handles ascending leap.");
        }
    }
}
