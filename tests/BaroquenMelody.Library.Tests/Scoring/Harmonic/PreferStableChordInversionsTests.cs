using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Scoring.Harmonic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Scoring.Harmonic;

[TestFixture]
internal sealed class PreferStableChordInversionsTests
{
    private PreferStableChordInversions _preferStableChordInversions = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

        _preferStableChordInversions = new PreferStableChordInversions(
            new ChordInversionIdentifier(chordNumberIdentifier, compositionConfiguration));
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Score_ReturnsExpectedPenalty(BaroquenChord nextChord, double expectedPenalty)
    {
        // act
        var penalty = _preferStableChordInversions.Score([], nextChord);

        // assert
        penalty.Should().Be(expectedPenalty);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            0d).SetName("A root position chord costs nothing.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.E1, MusicalTimeSpan.Half)
            ]),
            0d).SetName("A first inversion chord costs nothing.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
            ]),
            1d).SetName("A second inversion (six-four) chord costs one.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.FSharp4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.D2, MusicalTimeSpan.Half)
            ]),
            0d).SetName("An unidentifiable chord is neutral.");
    }
}
