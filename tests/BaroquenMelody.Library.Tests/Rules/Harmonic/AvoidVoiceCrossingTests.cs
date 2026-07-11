using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules.Harmonic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules.Harmonic;

[TestFixture]
internal sealed class AvoidVoiceCrossingTests
{
    private AvoidVoiceCrossing _avoidVoiceCrossing = null!;

    [SetUp]
    public void SetUp() => _avoidVoiceCrossing = new AvoidVoiceCrossing(TestCompositionConfigurations.Get(4));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _avoidVoiceCrossing.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    // register order (high -> low) for Get(4) is One, Two, Three, Four.
    private static IEnumerable<TestCaseData> TestCases()
    {
        var noPrecedingChords = new List<BaroquenChord>();

        // monotonically descending by register -> no crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("No crossing when voices descend by register");

        // soprano below alto -> crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Crossing when soprano falls below alto");

        // tenor above alto -> crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Crossing when tenor rises above alto");

        // bass above tenor -> crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.G2, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Crossing when bass rises above tenor");

        // equal pitches between adjacent voices are a unison, not a crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Equal adjacent pitches are not a crossing");

        // two voices, no crossing.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("No crossing with two voices in order");

        // two voices crossed.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Crossing with two voices inverted");

        // single voice cannot cross.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Single voice cannot cross");

        // the rule is a pure function of nextChord: a crossed preceding chord is irrelevant.
        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C2, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.C5, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Preceding chord is ignored when nextChord has no crossing");
    }
}
