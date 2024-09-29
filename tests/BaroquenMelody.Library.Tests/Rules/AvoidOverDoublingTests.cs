using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules;

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
        var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        var altoC3 = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half);
        var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);

        var tenorC2 = new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half);
        var tenorD2 = new BaroquenNote(Instrument.Three, Notes.D2, MusicalTimeSpan.Half);

        var bassC1 = new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half);
        var bassF1 = new BaroquenNote(Instrument.Four, Notes.F1, MusicalTimeSpan.Half);

        var duetChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3]);
        var duetChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3]);

        var trioChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3, tenorC2]);
        var trioChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3, tenorD2]);

        var quartetChordWithDuplicateNotes = new BaroquenChord([sopranoC4, altoC3, tenorC2, bassC1]);
        var quartetChordWithDifferingNotes = new BaroquenChord([sopranoC4, altoE3, tenorD2, bassF1]);

        var soloChord = new BaroquenChord([sopranoC4]);

        yield return new TestCaseData(new List<BaroquenChord>(), duetChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), duetChordWithDifferingNotes, true);

        yield return new TestCaseData(new List<BaroquenChord>(), trioChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), trioChordWithDifferingNotes, true);

        yield return new TestCaseData(new List<BaroquenChord>(), quartetChordWithDuplicateNotes, false);

        yield return new TestCaseData(new List<BaroquenChord>(), quartetChordWithDifferingNotes, true);

        yield return new TestCaseData(new List<BaroquenChord>(), soloChord, true);
    }
}
