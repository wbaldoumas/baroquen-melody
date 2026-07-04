using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Motifs;

[TestFixture]
internal sealed class MotifExtractorTests
{
    private CompositionConfiguration _compositionConfiguration = null!;

    private MotifExtractor _motifExtractor = null!;

    [SetUp]
    public void SetUp()
    {
        // Get(1) is C major: the scale index is C=0, D=1, E=2, F=3, G=4, A=5, B=6, with each octave adding 7.
        _compositionConfiguration = TestCompositionConfigurations.Get(1);
        _motifExtractor = new MotifExtractor(_compositionConfiguration);
    }

    [Test]
    public void Extract_EncodesAscendingScaleStepsAndDurations()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert: the head delta is 0; C->E and E->G are each +2 scale steps.
        motif.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(2, MusicalTimeSpan.Half),
            new MotivicGesture(2, MusicalTimeSpan.Half)
        );
    }

    [Test]
    public void Extract_FirstGestureDeltaIsAlwaysZeroAndCarriesTheHeadDuration()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Quarter),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert
        motif.Gestures[0].ScaleStepDelta.Should().Be(0);
        motif.Gestures[0].Duration.Should().Be(MusicalTimeSpan.Quarter);
    }

    [Test]
    public void Extract_EncodesDescendingIntervalsAsNegativeDeltas()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.D4, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert: G->E is -2 scale steps, E->D is -1.
        motif.Gestures.Select(gesture => gesture.ScaleStepDelta).Should().Equal(0, -2, -1);
    }

    [Test]
    public void Extract_CapturesPerNoteDurationsInOrder()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter),
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Whole)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert
        motif.Gestures.Select(gesture => gesture.Duration).Should().Equal(MusicalTimeSpan.Half, MusicalTimeSpan.Quarter, MusicalTimeSpan.Whole);
    }

    [Test]
    public void Extract_EncodesAnOctaveLeapAsSevenScaleSteps()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C5, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert: the scale index is monotonic across the full MIDI range, so an octave is +7 (no octave wrap).
        motif.Gestures[1].ScaleStepDelta.Should().Be(7);
    }

    [Test]
    public void Extract_EncodesADescendingOctaveAsNegativeSevenScaleSteps()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert
        motif.Gestures[1].ScaleStepDelta.Should().Be(-7);
    }

    [Test]
    public void Extract_EncodesRepeatedNotesAsZeroDeltas()
    {
        // arrange
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert
        motif.Gestures.Select(gesture => gesture.ScaleStepDelta).Should().Equal(0, 0, 0);
    }

    [Test]
    public void Extract_IgnoresOrnamentations()
    {
        // arrange: two identical principal lines, one carrying ornamentation that must not affect the motif.
        var plainLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Half)
        };

        var ornamentedHead = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.Mordent
        };

        ornamentedHead.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth));

        var ornamentedLine = new List<BaroquenNote>
        {
            ornamentedHead,
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Half)
        };

        // act
        var plainMotif = _motifExtractor.Extract(plainLine);
        var ornamentedMotif = _motifExtractor.Extract(ornamentedLine);

        // assert: ornaments contribute nothing, and the deltas/durations match the plain principal skeleton exactly.
        ornamentedMotif.Gestures.Should().Equal(plainMotif.Gestures);
        ornamentedMotif.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(2, MusicalTimeSpan.Half)
        );
    }

    [Test]
    public void Extract_SingleNote_ProducesASingleZeroDeltaGesture()
    {
        // arrange
        var voiceLine = new List<BaroquenNote> { new(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter) };

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        // assert
        motif.Gestures.Should().ContainSingle().Which.Should().Be(new MotivicGesture(0, MusicalTimeSpan.Quarter));
    }

    [Test]
    public void Extract_EmptyVoiceLine_ProducesAnEmptyMotif()
    {
        // act
        var motif = _motifExtractor.Extract(new List<BaroquenNote>());

        // assert
        motif.Gestures.Should().BeEmpty();
    }

    [Test]
    public void Extract_NullVoiceLine_ThrowsArgumentNullException()
    {
        // act
        var act = () => _motifExtractor.Extract(null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("voiceLine");
    }

    [Test]
    public void Extract_OutOfScaleNote_ThrowsArgumentException()
    {
        // arrange: C#4 is chromatic in C major, so its scale index is -1.
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half)
        };

        // act
        var act = () => _motifExtractor.Extract(voiceLine);

        // assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("voiceLine")
            .WithMessage("*not in the configured scale*");
    }

    [Test]
    public void Extract_OutOfScaleHeadNote_ThrowsArgumentException()
    {
        // arrange: the very first note is chromatic, exercising validation of the head (index 0).
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.D4, MusicalTimeSpan.Half)
        };

        // act
        var act = () => _motifExtractor.Extract(voiceLine);

        // assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("voiceLine")
            .WithMessage("*not in the configured scale*");
    }

    [Test]
    public void ExtractThenReanchorAtTheOriginalHead_ReconstructsTheVoiceLineExactly()
    {
        // arrange: an in-scale line spanning octaves; the head anchor plus relative deltas must rebuild it verbatim.
        var voiceLine = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Quarter),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)
        };

        var scale = _compositionConfiguration.Scale;
        var scaleNotes = scale.GetNotes();

        // act
        var motif = _motifExtractor.Extract(voiceLine);

        var reconstructed = new List<BaroquenNote>(motif.Gestures.Count);
        var scaleIndex = scale.IndexOf(voiceLine[0]);

        foreach (var gesture in motif.Gestures)
        {
            scaleIndex += gesture.ScaleStepDelta;

            // Index directly (no clamp): a sign or magnitude regression in the deltas must fail loudly here rather than
            // be masked into a boundary note. Range-clamping is an apply-time concern (Step 11.3), not part of this oracle.
            reconstructed.Add(new BaroquenNote(Instrument.One, scaleNotes[scaleIndex], gesture.Duration));
        }

        // assert: BaroquenNote.Equals matches Instrument, Raw, MusicalTimeSpan, and (empty) Ornamentations.
        reconstructed.Should().Equal(voiceLine);
    }
}
