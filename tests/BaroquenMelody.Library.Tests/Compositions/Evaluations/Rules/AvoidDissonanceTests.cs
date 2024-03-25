using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class AvoidDissonanceTests
{
    private AvoidDissonance _avoidDissonance = null!;

    [SetUp]
    public void SetUp() => _avoidDissonance = new AvoidDissonance();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(Chord currentChord, Chord nextChord, bool expectedResult) =>
        _avoidDissonance.Evaluate([currentChord], nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            // the current chord is unnecessary for this test, so it is set to empty
            var unusedChord = new Chord([]);

            var sopranoC4 = new Note(Voice.Soprano, Notes.C4);
            var altoE3 = new Note(Voice.Alto, Notes.E3);
            var tenorG2 = new Note(Voice.Tenor, Notes.G2);
            var bassC1 = new Note(Voice.Bass, Notes.C1);
            var sopranoB4 = new Note(Voice.Soprano, Notes.B4);
            var sopranoCSharp4 = new Note(Voice.Soprano, Notes.CSharp4);
            var bassASharp1 = new Note(Voice.Bass, Notes.ASharp1);

            var cMajor = new Chord([sopranoC4, altoE3, tenorG2, bassC1]);
            var eMinor = new Chord([altoE3, tenorG2, sopranoB4]);
            var cMajor7 = new Chord([sopranoB4, altoE3, tenorG2, bassC1]);
            var cSharpDiminished = new Chord([sopranoCSharp4, altoE3, tenorG2, bassASharp1]);
            var cSharpDiminishedMajor7 = new Chord([sopranoCSharp4, altoE3, tenorG2, bassC1]);

            yield return new TestCaseData(unusedChord, cMajor, true).SetName("Major triad is consonant.");

            yield return new TestCaseData(unusedChord, eMinor, true).SetName("Minor triad is consonant.");

            yield return new TestCaseData(unusedChord, cMajor7, false).SetName("Major 7th chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminished, false).SetName("Diminished chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminishedMajor7, false).SetName("Diminished major 7th chord is dissonant.");
        }
    }
}
