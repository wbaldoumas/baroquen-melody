using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidOverDoublingTests
{
    private AvoidOverDoubling _avoidOverDoubling;

    [SetUp]
    public void SetUp() => _avoidOverDoubling = new AvoidOverDoubling();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _avoidOverDoubling.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);

        var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);
        var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);

        var tenorC2 = new BaroquenNote(Voice.Tenor, Notes.C2);
        var tenorD2 = new BaroquenNote(Voice.Tenor, Notes.D2);

        var bassC1 = new BaroquenNote(Voice.Bass, Notes.C1);
        var bassF1 = new BaroquenNote(Voice.Bass, Notes.F1);

        var duetChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3]);
        var duetChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3]);

        var trioChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3, tenorC2]);
        var trioChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3, tenorD2]);

        var quartetChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3, tenorC2, bassC1]);
        var quartetChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3, tenorD2, bassF1]);

        yield return new TestCaseData(new List<BaroquenChord>(), duetChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), duetChordWithDifferingNotes, true);

        yield return new TestCaseData(new List<BaroquenChord>(), trioChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), trioChordWithDifferingNotes, true);

        yield return new TestCaseData(new List<BaroquenChord>(), quartetChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), quartetChordWithDifferingNotes, true);
    }
}
