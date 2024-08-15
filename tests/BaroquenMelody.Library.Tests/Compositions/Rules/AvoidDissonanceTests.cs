using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidDissonanceTests
{
    private AvoidDissonance _avoidDissonance = null!;

    [SetUp]
    public void SetUp() => _avoidDissonance = new AvoidDissonance();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(BaroquenChord currentChord, BaroquenChord nextChord, bool expectedResult) =>
        _avoidDissonance.Evaluate([currentChord], nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            // the current chord is unnecessary for this test, so it is set to empty
            var unusedChord = new BaroquenChord([]);

            var sopranoC4 = new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Voice.Three, Notes.G2, MusicalTimeSpan.Half);
            var bassC1 = new BaroquenNote(Voice.Four, Notes.C1, MusicalTimeSpan.Half);
            var sopranoB4 = new BaroquenNote(Voice.One, Notes.B4, MusicalTimeSpan.Half);
            var sopranoCSharp4 = new BaroquenNote(Voice.One, Notes.CSharp4, MusicalTimeSpan.Half);
            var bassASharp1 = new BaroquenNote(Voice.Four, Notes.ASharp1, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC4, altoE3, tenorG2, bassC1]);
            var eMinor = new BaroquenChord([altoE3, tenorG2, sopranoB4]);
            var cMajor7 = new BaroquenChord([sopranoB4, altoE3, tenorG2, bassC1]);
            var cSharpDiminished = new BaroquenChord([sopranoCSharp4, altoE3, tenorG2, bassASharp1]);
            var cSharpDiminishedMajor7 = new BaroquenChord([sopranoCSharp4, altoE3, tenorG2, bassC1]);

            yield return new TestCaseData(unusedChord, cMajor, true).SetName("Major triad is consonant.");

            yield return new TestCaseData(unusedChord, eMinor, true).SetName("Minor triad is consonant.");

            yield return new TestCaseData(unusedChord, cMajor7, false).SetName("Major 7th chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminished, false).SetName("Diminished chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminishedMajor7, false).SetName("Diminished major 7th chord is dissonant.");
        }
    }
}
