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
internal sealed class PreferContraryOuterVoiceMotionTests
{
    private PreferContraryOuterVoiceMotion _preferContraryOuterVoiceMotion = null!;

    [SetUp]
    public void SetUp() => _preferContraryOuterVoiceMotion = new PreferContraryOuterVoiceMotion(TestCompositionConfigurations.Get(2));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Score_ReturnsExpectedPenalty(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, double expectedPenalty)
    {
        var penalty = _preferContraryOuterVoiceMotion.Score(precedingChords, nextChord);

        penalty.Should().Be(expectedPenalty);
    }

    [Test]
    public void Score_ReturnsZero_ForASingleVoiceConfiguration()
    {
        // arrange
        var preferContraryOuterVoiceMotion = new PreferContraryOuterVoiceMotion(TestCompositionConfigurations.Get(1));

        var precedingChords = new List<BaroquenChord>
        {
            new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])
        };

        var nextChord = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]);

        // act
        var penalty = preferContraryOuterVoiceMotion.Score(precedingChords, nextChord);

        // assert
        penalty.Should().Be(0d);
    }

    // Get(2) outer voices by register: soprano = Instrument.One (C4-C6), bass = Instrument.Two (G2-G4).
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
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty for contrary outer-voice motion");

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
                new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty when the soprano is oblique");

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
            0d
        ).SetName("No penalty when the bass is oblique");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("Similar ascending outer-voice motion costs one");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("Similar descending outer-voice motion costs one");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("Similar motion is licensed when the bass leaps a perfect fourth");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("Similar motion is licensed when the bass leaps a perfect fifth");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("Similar motion is licensed when the bass leaps down a perfect fourth");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("Similar motion with a bass third is not licensed");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]),
            0d
        ).SetName("No penalty when an outer voice is absent from the next chord");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty when an outer voice is absent from the preceding chord");
    }
}
