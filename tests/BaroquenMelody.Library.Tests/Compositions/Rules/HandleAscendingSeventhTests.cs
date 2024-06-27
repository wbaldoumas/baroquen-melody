using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class HandleAscendingSeventhTests
{
    private HandleAscendingSeventh _handleAscendingSeventh = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.G5, Notes.G6),
                new(Voice.Alto, Notes.C4, Notes.G5)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            100
        );

        _handleAscendingSeventh = new HandleAscendingSeventh(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        // act
        var result = _handleAscendingSeventh.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoA3 = new BaroquenNote(Voice.Soprano, Notes.A3);
            var sopranoB3 = new BaroquenNote(Voice.Soprano, Notes.B3);
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);

            var cMajor = new BaroquenChord([sopranoC4, altoE3]);
            var eMinor = new BaroquenChord([sopranoB3, altoE3]);
            var aMinor = new BaroquenChord([sopranoA3, altoE3]);

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor, true).SetName("No need to handle ascending seventh if it's the first chord.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, eMinor, true).SetName("No need to handle ascending seventh if it's the second chord.");

            yield return new TestCaseData(new List<BaroquenChord> { aMinor, eMinor }, cMajor, true).SetName("Correctly handles ascending seventh.");

            yield return new TestCaseData(new List<BaroquenChord> { aMinor, eMinor }, aMinor, false).SetName("Incorrectly handles ascending seventh.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, eMinor }, aMinor, true).SetName("No need to handle ascending seventh if its descending.");
        }
    }
}
