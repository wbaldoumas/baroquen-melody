using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class ChordInversionIdentifierTests
{
    private ChordInversionIdentifier _chordInversionIdentifier = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get();

        _chordInversionIdentifier = new ChordInversionIdentifier(new ChordNumberIdentifier(compositionConfiguration), compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void IdentifyChordInversion_ReturnsExpectedInversion(BaroquenChord chord, ChordInversion expectedChordInversion)
    {
        // act
        var chordInversion = _chordInversionIdentifier.IdentifyChordInversion(chord);

        // assert
        chordInversion.Should().Be(expectedChordInversion);
    }

    [Test]
    public void IdentifyChordInversion_WhenChordVoicesNoConfiguredInstrument_ReturnsUnknown()
    {
        // arrange - a two-instrument configuration, but a chord voiced only by unconfigured instruments
        var compositionConfiguration = TestCompositionConfigurations.Get(2);
        var chordInversionIdentifier = new ChordInversionIdentifier(new ChordNumberIdentifier(compositionConfiguration), compositionConfiguration);

        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.E2, MusicalTimeSpan.Half)
        ]);

        // act
        var chordInversion = chordInversionIdentifier.IdentifyChordInversion(chord);

        // assert
        chordInversion.Should().Be(ChordInversion.Unknown);
    }

    [Test]
    public void IdentifyChordInversion_WhenIdentifiedNumberDoesNotMatchTheBassNote_ReturnsUnknown()
    {
        // arrange - a chord number identifier that disagrees with the chord's actual notes, leaving the
        // bass outside the identified triad's chord tones
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();

        mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>()).Returns(ChordNumber.V);

        var chordInversionIdentifier = new ChordInversionIdentifier(mockChordNumberIdentifier, compositionConfiguration);

        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        // act
        var chordInversion = chordInversionIdentifier.IdentifyChordInversion(chord);

        // assert
        chordInversion.Should().Be(ChordInversion.Unknown);
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
            ChordInversion.RootPosition).SetName("I with the root in the bass is in root position.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.E1, MusicalTimeSpan.Half)
            ]),
            ChordInversion.FirstInversion).SetName("I with the third in the bass is in first inversion.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
            ]),
            ChordInversion.SecondInversion).SetName("I with the fifth in the bass is in second inversion.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
            ]),
            ChordInversion.RootPosition).SetName("V with the root in the bass is in root position.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half)
            ]),
            ChordInversion.FirstInversion).SetName("A partially voiced chord takes its bass from the lowest voiced instrument.");

        yield return new TestCaseData(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.FSharp3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]),
            ChordInversion.Unknown).SetName("A chord outside the diatonic triads has an unknown inversion.");
    }
}
