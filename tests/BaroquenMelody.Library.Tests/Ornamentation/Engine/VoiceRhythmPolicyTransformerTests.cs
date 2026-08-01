using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine;

[TestFixture]
internal sealed class VoiceRhythmPolicyTransformerTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private VoiceRhythmLedger _voiceRhythmLedger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        _voiceRhythmLedger = new VoiceRhythmLedger();
    }

    [Test]
    public void Transform_WhenEnabled_ReplacesTheProbabilityGateWhereverItSits()
    {
        // arrange - the gate is the first input policy of most processors but sits behind a deterministic
        // precondition in the interval-decoration family; the substitution must be by instance, not position
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var precedingPolicy = Substitute.For<IInputPolicy<OrnamentationItem>>();
        var trailingPolicy = Substitute.For<IInputPolicy<OrnamentationItem>>();
        var inputPolicies = new[] { precedingPolicy, new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 60), trailingPolicy };

        // act
        var transformedPolicies = transformer.Transform(CreateOrnamentationConfiguration(OrnamentationType.DecorateInterval, 60), inputPolicies);

        // assert
        transformedPolicies.Should().HaveCount(3);
        transformedPolicies[0].Should().BeSameAs(precedingPolicy);
        transformedPolicies[1].Should().BeOfType<RoleAwareWantsToOrnament>();
        transformedPolicies[2].Should().BeSameAs(trailingPolicy);
    }

    [Test]
    public void Transform_WhenDisabled_ReturnsTheExactSamePolicyArray()
    {
        // arrange - graph parity when disabled: not equivalent policies, the same instances
        var transformer = CreateTransformer(voiceRhythmEnabled: false);
        var inputPolicies = new IInputPolicy<OrnamentationItem>[] { new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 60) };

        // act
        var transformedPolicies = transformer.Transform(CreateOrnamentationConfiguration(OrnamentationType.Run, 60), inputPolicies);

        // assert
        transformedPolicies.Should().BeSameAs(inputPolicies);
    }

    [Test]
    public void Transform_ForASubdividingFigure_BoostsTheFloridWeightAndSilencesHeldNotes()
    {
        // arrange - the trill belongs to the beat-subdividing tier the florid role attracts
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(OrnamentationType.Trill, 20),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 20)]);

        var gate = transformedPolicies[0];

        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var floridNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
        var standardNote = new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordHeldNote(heldNote);
        _voiceRhythmLedger.RecordFloridNote(floridNote);

        // act
        gate.ShouldProcess(CreateItem(heldNote));
        gate.ShouldProcess(CreateItem(floridNote));
        gate.ShouldProcess(CreateItem(standardNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(VoiceRhythmPolicyTransformer.HeldNoteProbability);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(VoiceRhythmPolicyTransformer.FloridSubdividingProbability);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(20);
    }

    [Test]
    public void Transform_ForANonSubdividingFigure_KeepsTheFloridWeightAtTheBaseProbability()
    {
        // arrange - the florid role boosts only the subdividing tier; a passing tone keeps its base weight
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(OrnamentationType.PassingTone, 80),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 80)]);

        var gate = transformedPolicies[0];
        var floridNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordFloridNote(floridNote);

        // act
        gate.ShouldProcess(CreateItem(floridNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(80);
    }

    [Test]
    public void CreateSustainGate_WhenEnabled_TiesHeldPairsDeterministically()
    {
        // arrange
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var sustainGate = transformer.CreateSustainGate();

        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var standardNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordHeldNote(heldNote);

        // act
        sustainGate.ShouldProcess(CreateItem(heldNote));
        sustainGate.ShouldProcess(CreateItem(standardNote));

        // assert
        sustainGate.Should().BeOfType<RoleAwareWantsToOrnament>();
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(VoiceRhythmPolicyTransformer.HeldSustainProbability);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(WantsToOrnament.DefaultProbability);
    }

    [Test]
    public void CreateSustainGate_WhenDisabled_ReturnsTheStandardGate()
    {
        // arrange
        var transformer = CreateTransformer(voiceRhythmEnabled: false);

        // act
        var sustainGate = transformer.CreateSustainGate();

        // assert
        sustainGate.Should().BeOfType<WantsToOrnament>();
    }

    private VoiceRhythmPolicyTransformer CreateTransformer(bool voiceRhythmEnabled)
    {
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled)
        };

        return new VoiceRhythmPolicyTransformer(_mockWeightedRandomBooleanGenerator, _voiceRhythmLedger, compositionConfiguration);
    }

    private static OrnamentationConfiguration CreateOrnamentationConfiguration(OrnamentationType ornamentationType, int probability) =>
        new(ornamentationType, ConfigurationStatus.Enabled, probability);

    private static OrnamentationItem CreateItem(BaroquenNote note) =>
        new(note.Instrument, [], new Beat(new BaroquenChord([note])), NextBeat: null);
}
