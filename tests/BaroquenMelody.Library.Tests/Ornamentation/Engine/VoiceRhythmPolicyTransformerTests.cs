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
using BaroquenMelody.Library.Ornamentation.Utilities;
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
    public void SubdividingOrnamentationTypes_MatchTheFiguresTheCalculatorRendersAsFullSixteenths()
    {
        // arrange - the tier is derived knowledge: a figure subdivides the beat when its 4/4 rendering makes
        // both the primary note and its ornamentation notes sixteenths. If this fails, an ornamentation was
        // added or re-spanned without deciding its rhythm-role tier - add it to (or consciously exclude it
        // from) the transformer's set.
        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        // act
        var fullSixteenthFigures = Enum.GetValues<OrnamentationType>()
            .Where(ornamentationType => IsFullSixteenthFigure(musicalTimeSpanCalculator, ornamentationType));

        // assert
        VoiceRhythmPolicyTransformer.SubdividingOrnamentationTypes.Should().BeEquivalentTo(fullSixteenthFigures);
    }

    [Test]
    public void Transform_TheSubdividingTierGates_ScaleByRecordedIntensity()
    {
        // arrange - the ground's escalation ramps the division figures themselves: a tier gate at base 20
        // scales to 6 under a calm intensity of 30
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(OrnamentationType.Trill, 20),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 20)]);

        var gate = transformedPolicies[0];
        var escalatedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordDivisionIntensity(escalatedNote, 30);

        // act
        gate.ShouldProcess(CreateItem(escalatedNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(6);
    }

    [Test]
    public void Transform_TheNonSubdividingGates_NeverScaleByIntensity()
    {
        // arrange - decoration coverage is near-saturated: scaling every figure's weight reshuffles which
        // processor claims a note without producing an arc (measured), so non-tier gates keep stock
        // weights at every intensity and the competition around the ramping tier stays constant
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(OrnamentationType.PassingTone, 40),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 40)]);

        var gate = transformedPolicies[0];
        var escalatedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordDivisionIntensity(escalatedNote, 30);

        // act
        gate.ShouldProcess(CreateItem(escalatedNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(40);
    }

    [Test]
    public void CreateSustainGate_NeverScalesByIntensity()
    {
        // arrange - a calm statement's reappearing ties ARE the escalation's quiet end: the sustain gate
        // must keep the stock weight for an intensity-carrying note rather than scale it away
        var transformer = CreateTransformer(voiceRhythmEnabled: true);
        var sustainGate = transformer.CreateSustainGate();
        var escalatedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordDivisionIntensity(escalatedNote, 60);

        // act
        sustainGate.ShouldProcess(CreateItem(escalatedNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(WantsToOrnament.DefaultProbability);
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

    // The 4/4 column is the canonical duple rendering (in the triple meters the same figures carry a longer
    // primary ahead of their sixteenths), and stamps and pass-managed types have no beat rendering at all,
    // which the calculator signals by throwing.
    private static bool IsFullSixteenthFigure(MusicalTimeSpanCalculator musicalTimeSpanCalculator, OrnamentationType ornamentationType)
    {
        try
        {
            return musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, Meter.FourFour) == MusicalTimeSpan.Sixteenth
                && musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour) == MusicalTimeSpan.Sixteenth;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}
