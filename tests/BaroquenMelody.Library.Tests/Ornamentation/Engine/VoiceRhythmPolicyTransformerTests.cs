using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
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

    [TestCase(TextureType.Walking, OrnamentationType.RepeatedNote, 15, VoiceRhythmPolicyTransformer.TextureFigureProbability)]
    [TestCase(TextureType.Walking, OrnamentationType.PassingTone, 80, VoiceRhythmPolicyTransformer.TextureFigureProbability)]
    [TestCase(TextureType.Walking, OrnamentationType.Mordent, 20, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.Pedal, 80, VoiceRhythmPolicyTransformer.TextureFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.OctavePedalArpeggio, 25, VoiceRhythmPolicyTransformer.TextureAccentFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.UpperOctavePedalArpeggio, 25, VoiceRhythmPolicyTransformer.TextureAccentFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.OctavePedalPassingTone, 25, VoiceRhythmPolicyTransformer.TextureAccentFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.UpperOctavePedalPassingTone, 25, VoiceRhythmPolicyTransformer.TextureAccentFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.Arpeggio, 25, VoiceRhythmPolicyTransformer.TextureFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.OctavePedal, 80, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.UpperOctavePedal, 80, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.Walking, OrnamentationType.Arpeggio, 25, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.PassingTone, 80, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.DecorateThird, 60, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.Chordal, OrnamentationType.RepeatedNote, 15, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.Chordal, OrnamentationType.Run, 80, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.None, OrnamentationType.RepeatedNote, 15, 15)]
    public void Transform_ResolvesTheTextureWeightFromTheConfiguredFamilyAtBuildTime(TextureType texture, OrnamentationType ornamentationType, int baseProbability, int expectedTextureWeight)
    {
        // arrange - the texture is a configuration constant: in-family figures are near-certain, everything
        // else is silenced (Chordal's empty family silences the whole voice), and with no texture configured
        // the weight stays neutral so a spurious mark would behave standardly rather than silently silence
        var transformer = CreateTransformer(voiceRhythmEnabled: true, texture);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(ornamentationType, baseProbability),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, baseProbability)]);

        var gate = transformedPolicies[0];
        var figurationNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordTextureFigurationNote(figurationNote);

        // act
        gate.ShouldProcess(CreateItem(figurationNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(expectedTextureWeight);
    }

    [Test]
    public void Transform_WithAnUnclassifiedTextureType_ThrowsInsteadOfInheritingSilence()
    {
        // arrange - a future TextureType must be classified deliberately: a silent fall-through would mute
        // the figuration voice with no figure family to carry it, the same discipline the subdividing set
        // demands of new ornamentation types
        var transformer = CreateTransformer(voiceRhythmEnabled: true, (TextureType)99);

        // act
        var act = () => transformer.Transform(
            CreateOrnamentationConfiguration(OrnamentationType.RepeatedNote, 15),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 15)]);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [TestCase(TextureType.BrokenChord, OrnamentationType.PassingTone, VoiceRhythmPolicyTransformer.PadGentleFigureProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.NeighborTone, VoiceRhythmPolicyTransformer.PadGentleFigureProbability)]
    [TestCase(TextureType.Walking, OrnamentationType.NeighborTone, VoiceRhythmPolicyTransformer.PadGentleFigureProbability)]
    [TestCase(TextureType.Chordal, OrnamentationType.PassingTone, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.Chordal, OrnamentationType.NeighborTone, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.None, OrnamentationType.PassingTone, 80)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.Run, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.Arpeggio, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.OctavePedal, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    [TestCase(TextureType.BrokenChord, OrnamentationType.DelayedNeighborTone, VoiceRhythmPolicyTransformer.HeldNoteProbability)]
    public void Transform_ResolvesThePadWeightFromTheGentleFamilyUnderTheFiguralTextures(TextureType texture, OrnamentationType ornamentationType, int expectedPadWeight)
    {
        // arrange - under the figural textures the gentle figures take the very-low pad weight and every
        // other figure stays silenced on pads (including the excluded delayed variants); Chordal is the
        // deliberate exception - its identity IS the figure-free accompaniment, so its pads never breathe;
        // under None a spurious pad mark stays NEUTRAL at the configured probability, the same discipline
        // as the texture weight. The note is recorded in the pad store ONLY, so every row also decides
        // that the pad branch itself is reached - a note in both stores could take the held branch's zero
        // and still satisfy the silent rows.
        var transformer = CreateTransformer(voiceRhythmEnabled: true, texture);
        var transformedPolicies = transformer.Transform(
            CreateOrnamentationConfiguration(ornamentationType, 80),
            [new WantsToOrnament(_mockWeightedRandomBooleanGenerator, 80)]);

        var gate = transformedPolicies[0];
        var padNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordTexturePadNote(padNote);

        // act
        gate.ShouldProcess(CreateItem(padNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(expectedPadWeight);
    }

    [Test]
    public void CreateSustainGate_TiesPadPairsAtTheHeldWeight()
    {
        // arrange - the gentle-figure weight belongs to the ornamentation gates alone: the sustain gate
        // must keep the deterministic tie weight for pads, or the breathing change would silently loosen
        // every pad hold instead of adding occasional motion
        var transformer = CreateTransformer(voiceRhythmEnabled: true, TextureType.BrokenChord);
        var sustainGate = transformer.CreateSustainGate();
        var padNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordTexturePadNote(padNote);

        // act
        sustainGate.ShouldProcess(CreateItem(padNote));

        // assert - pad-store-only, so this decides the sustain gate's OWN padProbability wiring: wiring
        // the gentle resolver in by mistake would surface as 20 here instead of the tie weight
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(VoiceRhythmPolicyTransformer.HeldSustainProbability);
    }

    [Test]
    public void PadGentleFigures_AreEvenSingleStepFiguresThatDecorateTheHoldItself()
    {
        // arrange - gentle must MEAN gentle, decided against the factory and the calculator rather than
        // the coarse count table (which six other figures would satisfy, including the deliberately
        // excluded delayed variants and the non-stepwise RepeatedNote): one sub-note exactly one scale
        // step away, an even un-syncopated split of the beat, translated on the pad's own note (never an
        // anticipation of the next), and outside the subdividing tier
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration);

        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        foreach (var ornamentationType in VoiceRhythmPolicyTransformer.PadGentleFigures)
        {
            var configuration = ornamentationProcessorConfigurationFactory
                .Create(CreateOrnamentationConfiguration(ornamentationType, 100))
                .Should().ContainSingle($"{ornamentationType} must be a single-configuration figure").Subject;

            configuration.Translations.Should().ContainSingle($"{ornamentationType} must add exactly one sub-note")
                .Which.Should().Match(static translation => Math.Abs(translation) == 1, "the sub-note must sit one scale step from the hold");
            configuration.ShouldTranslateOnCurrentNote.Should().BeTrue($"{ornamentationType} must decorate the hold itself, never anticipate the next note");

            musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, Meter.FourFour)
                .Should().Be(
                    musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour),
                    $"{ornamentationType} must split the beat evenly - a dotted, delayed shape is more intrusive than a pad should be");

            VoiceRhythmPolicyTransformer.SubdividingOrnamentationTypes.Should().NotContain(ornamentationType);
        }
    }

    [Test]
    public void CreateSustainGate_StaysNeutralForTextureFigurationNotes()
    {
        // arrange - the pad voices' deterministic ties come from the held store; a bare repeated figuration
        // note may tie at stock odds, never at a texture-specific weight
        var transformer = CreateTransformer(voiceRhythmEnabled: true, TextureType.Walking);
        var sustainGate = transformer.CreateSustainGate();
        var figurationNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordTextureFigurationNote(figurationNote);

        // act
        sustainGate.ShouldProcess(CreateItem(figurationNote));

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(WantsToOrnament.DefaultProbability);
    }

    [Test]
    public void WalkingFigures_AllRenderTheEvenQuarterTread()
    {
        // arrange - family membership is onset-spacing uniformity: a texture's audible identity is the
        // spacing of its attacks, so every walking member must render primary and sub-notes as even
        // quarters in 4/4. If this fails, a member was re-spanned without re-deciding its family.
        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        // act & assert
        foreach (var ornamentationType in VoiceRhythmPolicyTransformer.WalkingFigures)
        {
            musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, Meter.FourFour)
                .Should().Be(MusicalTimeSpan.Quarter, $"{ornamentationType} must tread even quarters");
            musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour)
                .Should().Be(MusicalTimeSpan.Quarter, $"{ornamentationType} must tread even quarters");
        }
    }

    [Test]
    public void BrokenChordFigures_AllRenderTheEvenEighthPattern()
    {
        // arrange - the broken-chord family's uniform grid is the even eighth: three sub-notes filling the
        // beat behind an eighth principal. DecorateThird is deliberately excluded for its mixed
        // sixteenth/eighth spacing - pinned below so its exclusion is a decision, not an accident.
        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        // act & assert
        foreach (var ornamentationType in VoiceRhythmPolicyTransformer.BrokenChordFigures)
        {
            musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, Meter.FourFour)
                .Should().Be(MusicalTimeSpan.Eighth, $"{ornamentationType} must pattern even eighths");
            musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour)
                .Should().Be(MusicalTimeSpan.Eighth, $"{ornamentationType} must pattern even eighths");
        }

        musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DecorateThird, Meter.FourFour, ornamentationStep: 0)
            .Should().Be(MusicalTimeSpan.Sixteenth, "DecorateThird's mixed spacing is why it sits outside the family");
    }

    [Test]
    public void TextureFamilies_NeverContainTheCadentialDecorateIntervalOrOverlapEachOther()
    {
        // arrange - DecorateInterval fires only at a strict V-to-I approach (cadential, near-zero site
        // supply under concentration), and the families answer different textures so they must not overlap
        VoiceRhythmPolicyTransformer.WalkingFigures.Should().NotContain(OrnamentationType.DecorateInterval);
        VoiceRhythmPolicyTransformer.BrokenChordFigures.Should().NotContain(OrnamentationType.DecorateInterval);
        VoiceRhythmPolicyTransformer.WalkingFigures.Intersect(VoiceRhythmPolicyTransformer.BrokenChordFigures).Should().BeEmpty();
    }

    [Test]
    public void BrokenChordAccentFigures_AreAProperSubsetOfTheFamilyThatNeverClaimsTheCarryingFigures()
    {
        // arrange - the accent tier only modulates weight WITHIN the family: an accent member outside
        // BrokenChordFigures would take the accent weight while flunking the family's spacing pins, and
        // an accent set that swallowed the carrying figures (Arpeggio, Pedal) would thin the fabric to
        // nothing - both must be decisions, never drift
        VoiceRhythmPolicyTransformer.BrokenChordAccentFigures.Should().BeSubsetOf(VoiceRhythmPolicyTransformer.BrokenChordFigures);
        VoiceRhythmPolicyTransformer.BrokenChordAccentFigures.Should().NotContain(OrnamentationType.Arpeggio);
        VoiceRhythmPolicyTransformer.BrokenChordAccentFigures.Should().NotContain(OrnamentationType.Pedal);
    }

    private VoiceRhythmPolicyTransformer CreateTransformer(bool voiceRhythmEnabled, TextureType texture = TextureType.None)
    {
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled),
            TextureConfiguration = new TextureConfiguration(texture)
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
