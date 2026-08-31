using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring.Melodic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Scoring.Melodic;

[TestFixture]
internal sealed class PreferLeapRecoveryTests
{
    private MelodicScoringRuleAdapter _preferLeapRecovery = null!;

    // The chord-level cases predate the melodic viewpoint; scoring through the adapter keeps them verbatim while
    // covering the rule and its per-voice aggregation together.
    [SetUp]
    public void SetUp() => _preferLeapRecovery = new MelodicScoringRuleAdapter(new PreferLeapRecovery(TestCompositionConfigurations.Get(2)));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Score_ReturnsExpectedPenalty(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, double expectedPenalty)
    {
        var penalty = _preferLeapRecovery.Score(precedingChords, nextChord);

        penalty.Should().Be(expectedPenalty);
    }

    // Get(2) is C major. A leap is three or more scale steps (C4 -> F4); recovery is a one- or two-step move in the
    // opposite direction. Instrument.Two is held stationary (no leap) except where a test exercises it.
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
            new List<BaroquenChord>(),
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty without preceding chords");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("No penalty with a single preceding chord");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.E4),
            0d
        ).SetName("No penalty when a leap recovers by step in the opposite direction");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.D4),
            0d
        ).SetName("No penalty when a leap recovers by a third in the opposite direction");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.G4),
            1d
        ).SetName("A leap continued in the same direction costs one");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.F4),
            1d
        ).SetName("A leap followed by a held note costs one");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.C4),
            1d
        ).SetName("A leap answered by an opposite leap costs one");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.D4),
            BuildNextChord(Notes.G4),
            0d
        ).SetName("No penalty when the previous move was not a leap");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.E4),
            BuildNextChord(Notes.G4),
            0d
        ).SetName("No penalty when the previous move was a third, just below the leap threshold");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.C4, Notes.F4),
            BuildNextChord(Notes.CSharp4),
            0d
        ).SetName("Notes outside the scale are not scored");

        yield return new TestCaseData(
            BuildPrecedingChords(Notes.F4, Notes.C4),
            BuildNextChord(Notes.D4),
            0d
        ).SetName("No penalty when a downward leap recovers by step upward");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]),
            2d
        ).SetName("Unrecovered leaps sum across voices");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("Voices absent from a context chord are not scored");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]),
                new([
                    new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            1d
        ).SetName("A voice absent from the earlier context chord is not scored");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("An out-of-scale note in the earliest context chord is not scored");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ])
            },
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ]),
            0d
        ).SetName("An out-of-scale note in the middle context chord is not scored");

        // Audit: search-scoring-random-1 - a held-harmony duplicate (the harmonic-rhythm hold) must not erase the leap.
        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new BaroquenChord(new BaroquenChord([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]))
            },
            BuildNextChord(Notes.C5),
            1d
        ).SetName("A leap held across a duplicated chord and then continued in the same direction costs one");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new BaroquenChord(new BaroquenChord([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]))
            },
            BuildNextChord(Notes.G4),
            0d
        ).SetName("A leap held across a duplicated chord and then recovered by step costs nothing");

        // The ground's close searches over already-decorated statements, so a held duplicate must stay
        // recognizable on raw pitches after the ornamentation pass has reshaped its paired beats.
        var heldChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);
        var decoratedDuplicate = new BaroquenChord(heldChord);

        decoratedDuplicate[Instrument.One].MusicalTimeSpan = MusicalTimeSpan.Quarter;
        decoratedDuplicate[Instrument.One].Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Quarter));

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                heldChord,
                decoratedDuplicate
            },
            BuildNextChord(Notes.C5),
            1d
        ).SetName("A leap held across a decorated duplicate and then continued in the same direction costs one");

        yield return new TestCaseData(
            new List<BaroquenChord>
            {
                new([
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
                ]),
                new([
                    new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
                ])
            },
            BuildNextChord(Notes.C5),
            0d
        ).SetName("A voice repeating its note over a fresh harmony is a real repeat, so no leap precedes the next move");
    }

    private static List<BaroquenChord> BuildPrecedingChords(Note firstNote, Note secondNote) =>
    [
        new([
            new BaroquenNote(Instrument.One, firstNote, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]),
        new([
            new BaroquenNote(Instrument.One, secondNote, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ])
    ];

    private static BaroquenChord BuildNextChord(Note note) => new([
        new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
    ]);
}
