using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Scoring;

[TestFixture]
internal sealed class PreferShortestVoiceMovementTests
{
    private PreferShortestVoiceMovement _preferShortestVoiceMovement = null!;

    [SetUp]
    public void SetUp() => _preferShortestVoiceMovement = new PreferShortestVoiceMovement(TestCompositionConfigurations.Get(2));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Score_ReturnsExpectedPenalty(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, double expectedPenalty)
    {
        var penalty = _preferShortestVoiceMovement.Score(precedingChords, nextChord);

        penalty.Should().Be(expectedPenalty);
    }

    // Get(2) is C major: scale-step distances are diatonic (C4 -> D4 = 1, C4 -> E4 = 2, C4 -> G4 = 4...).
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
            new List<BaroquenChord>(),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty without a preceding chord");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty when every voice holds its note");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("A single stepwise move costs one");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
            ]),
            3d
        ).SetName("Voice movements sum across voices");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            4d
        ).SetName("A leap costs its full scale-step distance");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("Voices absent from the preceding chord are not scored");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]),
            1d
        ).SetName("Voices absent from the next chord are not scored");
    }
}
