using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Rhythm;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class RoleAwareWantsToOrnamentTests
{
    private const int Probability = 40;

    private const int HeldProbability = 0;

    private const int FloridProbability = 70;

    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private VoiceRhythmLedger _voiceRhythmLedger = null!;

    private RoleAwareWantsToOrnament _roleAwareWantsToOrnament = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        _voiceRhythmLedger = new VoiceRhythmLedger();

        _roleAwareWantsToOrnament = new RoleAwareWantsToOrnament(
            _mockWeightedRandomBooleanGenerator,
            _voiceRhythmLedger,
            Probability,
            HeldProbability,
            FloridProbability);
    }

    [Test]
    public void ShouldProcess_WithAHeldNote_DrawsExactlyOnceAtTheHeldWeight()
    {
        // arrange
        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordHeldNote(heldNote);
        _mockWeightedRandomBooleanGenerator.IsTrue(HeldProbability).Returns(false);

        // act
        var result = _roleAwareWantsToOrnament.ShouldProcess(CreateItem(heldNote));

        // assert - the draw happens even though the weight silences the figure, preserving the stream
        result.Should().Be(InputPolicyResult.Reject);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(HeldProbability);
    }

    [Test]
    public void ShouldProcess_WithAFloridNote_DrawsExactlyOnceAtTheFloridWeight()
    {
        // arrange
        var floridNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordFloridNote(floridNote);
        _mockWeightedRandomBooleanGenerator.IsTrue(FloridProbability).Returns(true);

        // act
        var result = _roleAwareWantsToOrnament.ShouldProcess(CreateItem(floridNote));

        // assert
        result.Should().Be(InputPolicyResult.Continue);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(FloridProbability);
    }

    [Test]
    public void ShouldProcess_WithAnUnrecordedNote_DrawsExactlyOnceAtTheStandardWeight()
    {
        // arrange
        var standardNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _mockWeightedRandomBooleanGenerator.IsTrue(Probability).Returns(true);

        // act
        var result = _roleAwareWantsToOrnament.ShouldProcess(CreateItem(standardNote));

        // assert
        result.Should().Be(InputPolicyResult.Continue);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(Probability);
    }

    [Test]
    public void ShouldProcess_WhenTheInstrumentIsAbsentFromTheBeat_DrawsExactlyOnceAtTheStandardWeight()
    {
        // arrange - progressively voiced exposition chords flow through the same policies, so an absent
        // voice must take the standard weight rather than throw on the chord lookup
        var otherVoiceNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
        var item = new OrnamentationItem(Instrument.One, [], new Beat(new BaroquenChord([otherVoiceNote])), NextBeat: null);

        _mockWeightedRandomBooleanGenerator.IsTrue(Probability).Returns(false);

        // act
        var result = _roleAwareWantsToOrnament.ShouldProcess(item);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(Probability);
    }

    [TestCase(40, 60, 24)]
    [TestCase(40, 140, 56)]
    [TestCase(80, 140, 100)]
    public void ShouldProcess_WithARecordedIntensity_DrawsExactlyOnceAtTheScaledAndClampedWeight(int baseProbability, int intensity, int expectedWeight)
    {
        // arrange - a recorded intensity scales the resolved weight (clamped to the legal range) without
        // adding or removing a draw
        var scaledNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var gate = new RoleAwareWantsToOrnament(_mockWeightedRandomBooleanGenerator, _voiceRhythmLedger, baseProbability, HeldProbability, FloridProbability);

        _voiceRhythmLedger.RecordDivisionIntensity(scaledNote, intensity);

        // act
        gate.ShouldProcess(CreateItem(scaledNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(expectedWeight);
    }

    [Test]
    public void ShouldProcess_WithAFloridNoteCarryingAnIntensity_ScalesTheFloridWeight()
    {
        // arrange - the florid boost and the escalation compose: 70 at 140 clamps from 98
        var floridNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordFloridNote(floridNote);
        _voiceRhythmLedger.RecordDivisionIntensity(floridNote, 140);

        // act
        _roleAwareWantsToOrnament.ShouldProcess(CreateItem(floridNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(98);
    }

    [Test]
    public void ShouldProcess_WithScalingDisabled_IgnoresARecordedIntensity()
    {
        // arrange - the sustain gate's shape: intensities exist but must not move its weights
        var scaledNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var sustainShapedGate = new RoleAwareWantsToOrnament(
            _mockWeightedRandomBooleanGenerator,
            _voiceRhythmLedger,
            Probability,
            HeldProbability,
            FloridProbability,
            scaleByIntensity: false);

        _voiceRhythmLedger.RecordDivisionIntensity(scaledNote, 60);

        // act
        sustainShapedGate.ShouldProcess(CreateItem(scaledNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(Probability);
    }

    [Test]
    public void ShouldProcess_WithAHeldNoteCarryingAnIntensity_TheHeldWeightIsNeverScaled()
    {
        // arrange - held resolves before the intensity consult: suppression stays absolute
        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordHeldNote(heldNote);
        _voiceRhythmLedger.RecordDivisionIntensity(heldNote, 140);

        // act
        _roleAwareWantsToOrnament.ShouldProcess(CreateItem(heldNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(HeldProbability);
    }

    private static OrnamentationItem CreateItem(BaroquenNote note) =>
        new(note.Instrument, [], new Beat(new BaroquenChord([note])), NextBeat: null);
}
