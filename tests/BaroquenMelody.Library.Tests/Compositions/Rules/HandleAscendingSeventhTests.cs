using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
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
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

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
            var sopranoA3 = new BaroquenNote(Voice.One, Notes.A3, MusicalTimeSpan.Half);
            var sopranoB3 = new BaroquenNote(Voice.One, Notes.B3, MusicalTimeSpan.Half);
            var sopranoC4 = new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half);

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
