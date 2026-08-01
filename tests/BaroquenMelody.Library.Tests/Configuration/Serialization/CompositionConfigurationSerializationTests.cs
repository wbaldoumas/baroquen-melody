using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Serialization.JsonSerializerContexts;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Motifs.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Scoring.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BaroquenMelody.Library.Tests.Configuration.Serialization;

[TestFixture]
internal sealed class CompositionConfigurationSerializationTests
{
    [Test]
    public void Serialization_works_as_expected()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Accordion, ConfigurationStatus.Disabled),
                new(Instrument.Two, Notes.C3, Notes.G4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
                new(Instrument.Three, Notes.C2, Notes.G3, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
                new(Instrument.Four, Notes.C1, Notes.G2, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Aeolian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            AggregateScoringRuleConfiguration: new AggregateScoringRuleConfiguration(
                new HashSet<ScoringRuleConfiguration>
                {
                    new(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 3),
                    new(ScoringRule.PreferContraryOuterVoiceMotion, ConfigurationStatus.Disabled, Weight: 7),
                    new(ScoringRule.PreferLeapRecovery, ConfigurationStatus.EnabledAndLocked, Weight: 0)
                }
            )
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.Tonic.Should().Be(compositionConfiguration.Tonic);
        deserializedConfiguration.Mode.Should().Be(compositionConfiguration.Mode);
        deserializedConfiguration.Meter.Should().Be(compositionConfiguration.Meter);
        deserializedConfiguration.DefaultNoteTimeSpan.Should().Be(compositionConfiguration.DefaultNoteTimeSpan);
        deserializedConfiguration.MinimumMeasures.Should().Be(compositionConfiguration.MinimumMeasures);

        deserializedConfiguration.InstrumentConfigurations.Should().HaveCount(4);

        foreach (var deserializedInstrumentConfiguration in deserializedConfiguration.InstrumentConfigurations)
        {
            var originalInstrumentConfiguration = compositionConfiguration.InstrumentConfigurations.First(instrumentConfiguration =>
                instrumentConfiguration.Instrument == deserializedInstrumentConfiguration.Instrument
            );

            deserializedInstrumentConfiguration.Instrument.Should().Be(originalInstrumentConfiguration.Instrument);
            deserializedInstrumentConfiguration.MinNote.Should().Be(originalInstrumentConfiguration.MinNote);
            deserializedInstrumentConfiguration.MaxNote.Should().Be(originalInstrumentConfiguration.MaxNote);
            deserializedInstrumentConfiguration.MidiProgram.Should().Be(originalInstrumentConfiguration.MidiProgram);
            deserializedInstrumentConfiguration.IsEnabled.Should().Be(originalInstrumentConfiguration.IsEnabled);
        }

        deserializedConfiguration.PhrasingConfiguration.PhraseLengths.Should().BeEquivalentTo(PhrasingConfiguration.Default.PhraseLengths);
        deserializedConfiguration.PhrasingConfiguration.MaxPhraseRepetitions.Should().Be(PhrasingConfiguration.Default.MaxPhraseRepetitions);
        deserializedConfiguration.PhrasingConfiguration.MinPhraseRepetitionPoolSize.Should().Be(PhrasingConfiguration.Default.MinPhraseRepetitionPoolSize);
        deserializedConfiguration.PhrasingConfiguration.PhraseRepetitionProbability.Should().Be(PhrasingConfiguration.Default.PhraseRepetitionProbability);

        deserializedConfiguration.AggregateCompositionRuleConfiguration.Configurations.Should().HaveCount(AggregateCompositionRuleConfiguration.Default.Configurations.Count);

        foreach (var deserializedCompositionRuleConfiguration in deserializedConfiguration.AggregateCompositionRuleConfiguration.Configurations)
        {
            var originalCompositionRuleConfiguration = compositionConfiguration.AggregateCompositionRuleConfiguration.Configurations.First(compositionRuleConfiguration =>
                compositionRuleConfiguration.Rule == deserializedCompositionRuleConfiguration.Rule
            );

            deserializedCompositionRuleConfiguration.Rule.Should().Be(originalCompositionRuleConfiguration.Rule);
            deserializedCompositionRuleConfiguration.IsEnabled.Should().Be(originalCompositionRuleConfiguration.IsEnabled);
            deserializedCompositionRuleConfiguration.Strictness.Should().Be(originalCompositionRuleConfiguration.Strictness);
        }

        deserializedConfiguration.AggregateOrnamentationConfiguration.Configurations.Should().HaveCount(AggregateOrnamentationConfiguration.Default.Configurations.Count);

        foreach (var deserializedOrnamentationConfiguration in deserializedConfiguration.AggregateOrnamentationConfiguration.Configurations)
        {
            var originalOrnamentationConfiguration = compositionConfiguration.AggregateOrnamentationConfiguration.Configurations.First(ornamentationConfiguration =>
                ornamentationConfiguration.OrnamentationType == deserializedOrnamentationConfiguration.OrnamentationType
            );

            deserializedOrnamentationConfiguration.OrnamentationType.Should().Be(originalOrnamentationConfiguration.OrnamentationType);
            deserializedOrnamentationConfiguration.IsEnabled.Should().Be(originalOrnamentationConfiguration.IsEnabled);
            deserializedOrnamentationConfiguration.Probability.Should().Be(originalOrnamentationConfiguration.Probability);
        }

        deserializedConfiguration.AggregateScoringRuleConfiguration.Should().NotBeNull();
        deserializedConfiguration.AggregateScoringRuleConfiguration!.Configurations.Should().HaveCount(compositionConfiguration.AggregateScoringRuleConfiguration!.Configurations.Count);

        foreach (var deserializedScoringRuleConfiguration in deserializedConfiguration.AggregateScoringRuleConfiguration.Configurations)
        {
            var originalScoringRuleConfiguration = compositionConfiguration.AggregateScoringRuleConfiguration.Configurations.First(scoringRuleConfiguration =>
                scoringRuleConfiguration.Rule == deserializedScoringRuleConfiguration.Rule
            );

            deserializedScoringRuleConfiguration.Rule.Should().Be(originalScoringRuleConfiguration.Rule);
            deserializedScoringRuleConfiguration.Status.Should().Be(originalScoringRuleConfiguration.Status);
            deserializedScoringRuleConfiguration.Weight.Should().Be(originalScoringRuleConfiguration.Weight);
        }
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_scoring_rules_yields_a_null_scoring_configuration()
    {
        // arrange: a configuration saved before scoring rules existed has no scoring property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.AggregateScoringRuleConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.AggregateScoringRuleConfiguration.Should().BeNull();
    }

    [Test]
    public void Serialization_preserves_the_motif_development_configuration()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            MotifDevelopmentConfiguration: new MotifDevelopmentConfiguration(
                Enabled: true,
                AllowedTransforms: [MotifTransform.Invert],
                DevelopmentProbability: 33,
                Scope: MotifDevelopmentScope.AllVoices
            )
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.MotifDevelopmentConfiguration.Should().NotBeNull();
        deserializedConfiguration.MotifDevelopmentConfiguration!.Enabled.Should().BeTrue();
        deserializedConfiguration.MotifDevelopmentConfiguration.AllowedTransforms.Should().Equal(MotifTransform.Invert);
        deserializedConfiguration.MotifDevelopmentConfiguration.DevelopmentProbability.Should().Be(33);
        deserializedConfiguration.MotifDevelopmentConfiguration.Scope.Should().Be(MotifDevelopmentScope.AllVoices);
    }

    [Test]
    public void Serialization_preserves_the_harmonic_rhythm_configuration()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            HarmonicRhythmConfiguration: new HarmonicRhythmConfiguration(Enabled: false)
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.HarmonicRhythmConfiguration.Should().NotBeNull();
        deserializedConfiguration.HarmonicRhythmConfiguration!.Enabled.Should().BeFalse();
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_harmonic_rhythm_yields_a_null_harmonic_rhythm_configuration()
    {
        // arrange: a configuration saved before variable harmonic rhythm existed has no such property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.HarmonicRhythmConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.HarmonicRhythmConfiguration.Should().BeNull();
    }

    [Test]
    public void Serialization_preserves_the_suspension_configuration()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            SuspensionConfiguration: new SuspensionConfiguration(Enabled: false, Probability: 25)
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.SuspensionConfiguration.Should().NotBeNull();
        deserializedConfiguration.SuspensionConfiguration!.Enabled.Should().BeFalse();
        deserializedConfiguration.SuspensionConfiguration.Probability.Should().Be(25);
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_suspensions_yields_a_null_suspension_configuration()
    {
        // arrange: a configuration saved before suspensions existed has no such property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.SuspensionConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.SuspensionConfiguration.Should().BeNull();
    }

    [Test]
    public void Tonicization_configuration_round_trips_through_serialization()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            TonicizationConfiguration: new TonicizationConfiguration(Enabled: false, Probability: 55)
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.TonicizationConfiguration.Should().NotBeNull();
        deserializedConfiguration.TonicizationConfiguration!.Enabled.Should().BeFalse();
        deserializedConfiguration.TonicizationConfiguration.Probability.Should().Be(55);
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_tonicization_yields_a_null_tonicization_configuration()
    {
        // arrange: a configuration saved before tonicization existed has no such property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.TonicizationConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.TonicizationConfiguration.Should().BeNull();
    }

    [Test]
    public void Ground_bass_configuration_round_trips_through_serialization()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca, Modulate: false)
        );

        // act
        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var deserializedConfiguration = JsonSerializer.Deserialize(serializedConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.GroundBassConfiguration.Should().NotBeNull();
        deserializedConfiguration.GroundBassConfiguration!.Enabled.Should().BeTrue();
        deserializedConfiguration.GroundBassConfiguration.Pattern.Should().Be(GroundBass.Romanesca);
        deserializedConfiguration.GroundBassConfiguration.Modulate.Should().BeFalse("a disabled journey must survive the round-trip");
    }

    [Test]
    public void Deserialization_of_a_ground_bass_configuration_without_a_pattern_yields_a_null_pattern()
    {
        // arrange: a configuration saved before pattern selection existed carries only the Enabled flag,
        // which must deserialize to the composer's free draw rather than fail or pin a pattern.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca)
        );

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson[nameof(CompositionConfiguration.GroundBassConfiguration)]!
            .AsObject()
            .Remove(nameof(GroundBassConfiguration.Pattern));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.GroundBassConfiguration.Should().NotBeNull();
        deserializedConfiguration.GroundBassConfiguration!.Enabled.Should().BeTrue();
        deserializedConfiguration.GroundBassConfiguration.Pattern.Should().BeNull();
    }

    [Test]
    public void Deserialization_of_a_ground_bass_configuration_without_a_modulate_flag_defaults_to_modulating()
    {
        // arrange: a configuration saved before modulation existed carries no Modulate property, and must
        // deserialize to the default journey rather than fail or silently pin the composition home.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100,
            GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca, Modulate: false)
        );

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson[nameof(CompositionConfiguration.GroundBassConfiguration)]!
            .AsObject()
            .Remove(nameof(GroundBassConfiguration.Modulate));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.GroundBassConfiguration.Should().NotBeNull();
        deserializedConfiguration.GroundBassConfiguration!.Enabled.Should().BeTrue();
        deserializedConfiguration.GroundBassConfiguration.Pattern.Should().Be(GroundBass.Romanesca);
        deserializedConfiguration.GroundBassConfiguration.Modulate.Should().BeTrue("a legacy save must take the default journey");
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_ground_bass_yields_a_null_ground_bass_configuration()
    {
        // arrange: a configuration saved before the ground bass form existed has no such property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.GroundBassConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.GroundBassConfiguration.Should().BeNull();
    }

    [Test]
    public void Deserialization_of_a_legacy_configuration_without_motif_development_yields_a_null_motif_development_configuration()
    {
        // arrange: a configuration saved before motivic development existed has no such property at all.
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
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

        var serializedConfiguration = JsonSerializer.Serialize(compositionConfiguration, CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration);
        var legacyConfigurationJson = JsonNode.Parse(serializedConfiguration)!.AsObject();

        legacyConfigurationJson.Remove(nameof(CompositionConfiguration.MotifDevelopmentConfiguration));

        // act
        var deserializedConfiguration = JsonSerializer.Deserialize(legacyConfigurationJson.ToJsonString(), CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration)!;

        // assert
        deserializedConfiguration.MotifDevelopmentConfiguration.Should().BeNull();
    }
}
