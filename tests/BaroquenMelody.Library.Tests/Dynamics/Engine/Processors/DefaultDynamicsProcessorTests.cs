using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Processors;
using BaroquenMelody.Library.Dynamics.Engine.Utilities;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Processors;

[TestFixture]
internal sealed class DefaultDynamicsProcessorTests
{
    private IVelocityCalculator _mockVelocityCalculator = null!;

    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockVelocityCalculator = Substitute.For<IVelocityCalculator>();
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
    }

    [Test]
    [TestCase(50, 50, 50, true, 50)]
    [TestCase(50, 75, 50, true, 51)]
    [TestCase(50, 75, 75, true, 74)]
    [TestCase(50, 75, 65, true, 66)]
    [TestCase(50, 75, 65, false, 64)]
    public void Process_generates_expected_velocity(byte minVelocity, byte maxVelocity, byte precedingVelocity, bool randomBoolean, byte expectedVelocity)
    {
        // arrange
        var instrumentConfiguration = new InstrumentConfiguration(
            Instrument.One,
            Notes.C4,
            Notes.C6,
            MinVelocity: new SevenBitNumber(minVelocity),
            MaxVelocity: new SevenBitNumber(maxVelocity),
            GeneralMidi2Program.AcousticGrandPiano,
            ConfigurationStatus.Enabled
        );

        var configuration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration> { instrumentConfiguration },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );

        var defaultDynamicsProcessor = new DefaultDynamicsProcessor(
            _mockVelocityCalculator,
            _mockWeightedRandomBooleanGenerator,
            configuration
        );

        _mockVelocityCalculator.GetPrecedingVelocity(Arg.Any<FixedSizeList<Beat>>(), Arg.Any<Instrument>()).Returns(new SevenBitNumber(precedingVelocity));
        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(randomBoolean);

        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            NextBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            ProcessedInstruments = []
        };

        // act
        defaultDynamicsProcessor.Process(item);

        // assert
        item.CurrentBeat[Instrument.One].Velocity.Should().Be(new SevenBitNumber(expectedVelocity));
    }
}
