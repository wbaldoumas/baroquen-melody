using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Infrastructure.Serialization.JsonSerializerContexts;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using System.Text.Json;

namespace BaroquenMelody.Library.Tests.Infrastructure.Serialization;

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
                new(Instrument.One, Notes.C4, Notes.G5, GeneralMidi2Program.Accordion, false),
                new(Instrument.Two, Notes.C3, Notes.G4),
                new(Instrument.Three, Notes.C2, Notes.G3),
                new(Instrument.Four, Notes.C1, Notes.G2)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Aeolian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
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
    }
}
