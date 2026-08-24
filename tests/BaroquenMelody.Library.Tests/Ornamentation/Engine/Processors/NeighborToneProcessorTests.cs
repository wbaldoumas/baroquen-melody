using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class NeighborToneProcessorTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator;

    private OrnamentationProcessor _neighborToneProcessor;

    private CompositionConfiguration _compositionConfiguration = null!;

    private OrnamentationProcessorConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(3);

        _compositionConfiguration = compositionConfiguration;

        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        var ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            _mockWeightedRandomBooleanGenerator,
            compositionConfiguration,
            Substitute.For<ILogger>()
        );

        var configuration = ornamentationProcessorConfigurationFactory.Create(
            new OrnamentationConfiguration(
                OrnamentationType.NeighborTone,
                ConfigurationStatus.Enabled,
                Probability: 100
            )
        ).First();

        _configuration = configuration;

        _neighborToneProcessor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configuration);
    }

    // Audit: ornamentation-engine-4
    // Instrument.One's floor is C4. On the inverting draw a neighbor tone on a repeated C4 emits B3, below the
    // configured range, and no input policy guards the inverted -1 offset.
    [Test]
    public void Process_never_emits_a_lower_neighbor_below_the_instrument_range_when_every_input_policy_passes()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(true);
        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(true);

        var inputPolicyResults = _configuration.InputPolicies.Select(policy => policy.ShouldProcess(ornamentationItem)).ToList();

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert: either a range guard rejects the site, or every emitted sub-note stays inside the instrument range.
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        var subNotesOutsideRange = noteToAssert.Ornamentations
            .Select(static ornamentation => ornamentation.Raw)
            .Where(raw => !_compositionConfiguration.IsNoteInInstrumentRange(Instrument.One, raw))
            .ToList();

        (inputPolicyResults.Contains(InputPolicyResult.Reject) || subNotesOutsideRange.Count == 0)
            .Should()
            .BeTrue($"a neighbor tone at the instrument floor must be rejected or kept in range, but every input policy passed and the sub-notes {string.Join(", ", subNotesOutsideRange)} fall outside the instrument range");
    }

    [Test]
    public void Process_applies_upper_neighbor_tone_ornamentation_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(false);

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.NeighborTone);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }

    [Test]
    public void Process_applies_lower_neighbor_tone_ornamentation_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(true);

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.NeighborTone);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }
}
