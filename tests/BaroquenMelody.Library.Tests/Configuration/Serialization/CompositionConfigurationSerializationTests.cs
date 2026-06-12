using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Serialization.JsonSerializerContexts;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
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
            AggregateScoringRuleConfiguration: AggregateScoringRuleConfiguration.Default
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
        deserializedConfiguration.AggregateScoringRuleConfiguration!.Configurations.Should().HaveCount(AggregateScoringRuleConfiguration.Default.Configurations.Count);

        foreach (var deserializedScoringRuleConfiguration in deserializedConfiguration.AggregateScoringRuleConfiguration.Configurations)
        {
            var originalScoringRuleConfiguration = AggregateScoringRuleConfiguration.Default.Configurations.First(scoringRuleConfiguration =>
                scoringRuleConfiguration.Rule == deserializedScoringRuleConfiguration.Rule
            );

            deserializedScoringRuleConfiguration.Rule.Should().Be(originalScoringRuleConfiguration.Rule);
            deserializedScoringRuleConfiguration.IsEnabled.Should().Be(originalScoringRuleConfiguration.IsEnabled);
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
}
