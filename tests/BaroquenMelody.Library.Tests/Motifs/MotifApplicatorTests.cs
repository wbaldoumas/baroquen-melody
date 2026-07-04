using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Motifs;

[TestFixture]
internal sealed class MotifApplicatorTests
{
    private CompositionConfiguration _compositionConfiguration = null!;

    private MotifApplicator _motifApplicator = null!;

    private BaroquenScale _scale = null!;

    [SetUp]
    public void SetUp()
    {
        // Get(1) is C major with Instrument.One ranged C4..C6.
        _compositionConfiguration = TestCompositionConfigurations.Get(1);
        _motifApplicator = new MotifApplicator(_compositionConfiguration);
        _scale = _compositionConfiguration.Scale;
    }

    [Test]
    public void Apply_IdentityMotifAtOriginalAnchor_ReconstructsVoiceLineVerbatim()
    {
        // arrange: an in-range line; extracting then re-anchoring at its own head must reproduce it exactly.
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Quarter),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)
        };

        var motif = new MotifExtractor(_compositionConfiguration).Extract(voiceLine);
        var anchoredMotif = new AnchoredMotif(motif, _scale.IndexOf(voiceLine[0]), Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert: BaroquenNote.Equals matches Instrument, Raw, MusicalTimeSpan, and (empty) Ornamentations.
        reconstructed.Should().Equal(voiceLine);
    }

    [Test]
    public void Apply_EmptyMotif_ReturnsEmptyList()
    {
        // arrange
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(new Motif([]), anchorScaleIndex, Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().BeEmpty();
    }

    [Test]
    public void Apply_SingleGestureMotif_ReturnsSingleNoteAtTheAnchorPitch()
    {
        // arrange
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter));
        var anchoredMotif = new AnchoredMotif(new Motif([new MotivicGesture(0, MusicalTimeSpan.Quarter)]), anchorScaleIndex, Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().Equal(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter));
    }

    [Test]
    public void Apply_AscendingOverflow_ClampsToTheInstrumentMaxNote()
    {
        // arrange: A5 + 2 scale steps lands on the ceiling C6; + 4 overflows and must clamp back to C6.
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.A5, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(2, MusicalTimeSpan.Half), new MotivicGesture(4, MusicalTimeSpan.Half)]),
            anchorScaleIndex,
            Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.A5, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half)
        );

        reconstructed.Should().OnlyContain(note => _compositionConfiguration.IsNoteInInstrumentRange(Instrument.One, note.Raw));
    }

    [Test]
    public void Apply_DescendingUnderflow_ClampsToTheInstrumentMinNote()
    {
        // arrange: C4 is the floor; descending deltas must clamp back up to C4.
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(-2, MusicalTimeSpan.Half), new MotivicGesture(-4, MusicalTimeSpan.Half)]),
            anchorScaleIndex,
            Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );
    }

    [Test]
    public void Apply_OverflowThenReturn_UsesFromAnchorPrefixSum_NotPerStepDrift()
    {
        // arrange: anchored at the ceiling C6 with deltas [0, +2, -2], the line lifts past C6 then returns to it.
        // From-anchor prefix sum yields [C6, C6, C6]; a per-step fold that rebased at the clamp would emit A5 last.
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(2, MusicalTimeSpan.Half), new MotivicGesture(-2, MusicalTimeSpan.Half)]),
            anchorScaleIndex,
            Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half)
        );
    }

    [Test]
    public void Apply_UnderflowThenReturn_UsesFromAnchorPrefixSum_NotPerStepDrift()
    {
        // arrange: anchored at the floor C4 with deltas [0, -2, +2], the line dips below C4 then returns. From-anchor
        // prefix sum yields [C4, C4, C4]; a per-step fold rebased at the clamp would emit E4 as the final note.
        var anchorScaleIndex = _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(-2, MusicalTimeSpan.Half), new MotivicGesture(2, MusicalTimeSpan.Half)]),
            anchorScaleIndex,
            Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );
    }

    [Test]
    public void Apply_InvertedMotifOverflowingTheRange_ClampsEveryNoteWithinIt()
    {
        // arrange: [C5,E5,G5] inverts to a descending [0,-2,-2] contour; anchored at the floor C4 it underflows, so
        // every note must clamp into the instrument range rather than fall below C4.
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.G5, MusicalTimeSpan.Half)
        };

        var inverted = MotifTransformations.Invert(new MotifExtractor(_compositionConfiguration).Extract(voiceLine));
        var anchoredMotif = new AnchoredMotif(inverted, _scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)), Instrument.One);

        // act
        var reconstructed = _motifApplicator.Apply(anchoredMotif);

        // assert: the inverted contour underflows from C4, so every note pins exactly to the floor C4.
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        reconstructed.Should().OnlyContain(note => _compositionConfiguration.IsNoteInInstrumentRange(Instrument.One, note.Raw));
    }

    [Test]
    public void Apply_CarriesGestureDurationsAndInstrument_WithoutOrnamentation()
    {
        // arrange: a different voice (Instrument.Two, range G2..G4) proves the instrument is carried, not hardcoded.
        var configuration = TestCompositionConfigurations.Get(2);
        var applicator = new MotifApplicator(configuration);
        var anchorScaleIndex = configuration.Scale.IndexOf(new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half));
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(1, MusicalTimeSpan.Quarter), new MotivicGesture(-1, MusicalTimeSpan.Eighth)]),
            anchorScaleIndex,
            Instrument.Two);

        // act
        var reconstructed = applicator.Apply(anchoredMotif);

        // assert
        reconstructed.Select(note => note.MusicalTimeSpan).Should().Equal(MusicalTimeSpan.Half, MusicalTimeSpan.Quarter, MusicalTimeSpan.Eighth);
        reconstructed.Should().OnlyContain(note => note.Instrument == Instrument.Two);
        reconstructed.Should().OnlyContain(note => note.Ornamentations.Count == 0);
    }

    [Test]
    public void Apply_NullAnchoredMotif_ThrowsArgumentNullException()
    {
        // act
        var act = () => _motifApplicator.Apply(null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("anchoredMotif");
    }

    [Test]
    public void Apply_WithChromaticInstrumentBounds_ClampsToTheNearestInScaleBoundaryNotes()
    {
        // arrange: an instrument ranged on chromatic bounds C#4..C#5 (neither is a C-major scale note). The window must
        // be the first scale note >= C#4 (D4) and the last scale note <= C#5 (C5) - which scale.IndexOf(MinNote/MaxNote)
        // cannot find, since those bounds are not in the scale.
        var configuration = ConfigurationWithInstrumentRange(Notes.CSharp4, Notes.CSharp5);
        var applicator = new MotifApplicator(configuration);

        // a motif anchored below the window (C4) that then leaps far above it (+10 scale steps).
        var anchoredMotif = new AnchoredMotif(
            new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(10, MusicalTimeSpan.Half)]),
            configuration.Scale.IndexOf(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)),
            Instrument.One);

        // act
        var reconstructed = applicator.Apply(anchoredMotif);

        // assert: note 0 (below the window) clamps up to D4; note 1 (above) clamps down to C5.
        reconstructed.Should().Equal(
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half)
        );
    }

    private static CompositionConfiguration ConfigurationWithInstrumentRange(Note minNote, Note maxNote) => new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, minNote, maxNote, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        },
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 100
    );
}
