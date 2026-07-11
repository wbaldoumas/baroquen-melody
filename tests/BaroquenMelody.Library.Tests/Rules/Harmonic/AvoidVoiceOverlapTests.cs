using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules.Harmonic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Rules.Harmonic;

[TestFixture]
internal sealed class AvoidVoiceOverlapTests
{
    private AvoidVoiceOverlap _avoidVoiceOverlap = null!;

    [SetUp]
    public void SetUp() => _avoidVoiceOverlap = new AvoidVoiceOverlap(TestCompositionConfigurations.Get(4));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _avoidVoiceOverlap.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    // register order (high -> low) for Get(4) is One, Two, Three, Four.
    private static IEnumerable<TestCaseData> TestCases()
    {
        // no preceding chord -> nothing to overlap.
        yield return new TestCaseData(
            new List<BaroquenChord>(),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("No preceding chord cannot overlap");

        // both voices step in parallel, staying clear of each other's previous positions.
        yield return new TestCaseData(
            Preceding(("One", Notes.C4), ("Two", Notes.C3)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Parallel stepwise motion does not overlap");

        // the lower voice rises above the upper voice's previous note (but does not cross it now).
        yield return new TestCaseData(
            Preceding(("One", Notes.C4), ("Two", Notes.C3)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E4, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Lower voice overlaps above upper voice's previous note");

        // the upper voice falls below the lower voice's previous note (but does not cross it now).
        yield return new TestCaseData(
            Preceding(("One", Notes.C4), ("Two", Notes.C3)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.A2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G2, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Upper voice overlaps below lower voice's previous note");

        // moving exactly to the adjacent voice's previous note is allowed (overlap is strictly beyond).
        yield return new TestCaseData(
            Preceding(("One", Notes.C4), ("Two", Notes.C3)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Touching the adjacent voice's previous note is not an overlap");

        // a three-voice overlap isolated to the inner (alto/tenor) pair.
        yield return new TestCaseData(
            Preceding(("One", Notes.C5), ("Two", Notes.C4), ("Three", Notes.C3)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.E4, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Inner-voice overlap is detected with three voices");

        // all four voices shift up a whole step together -> no overlap.
        yield return new TestCaseData(
            Preceding(("One", Notes.C5), ("Two", Notes.C4), ("Three", Notes.C3), ("Four", Notes.C2)),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.D3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.D2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Uniform stepwise motion across four voices does not overlap");
    }

    private static List<BaroquenChord> Preceding(params (string Instrument, Note Raw)[] notes) =>
    [
        new(notes.Select(note => new BaroquenNote(
            Enum.Parse<Instrument>(note.Instrument),
            note.Raw,
            MusicalTimeSpan.Half)).ToList())
    ];
}
