using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class NeighborToneProcessorTests
{
    private IMusicalTimeSpanCalculator _mockMusicalTimeSpanCalculator;

    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator;

    private NeighborToneProcessor _neighborToneProcessor;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(3);

        _mockMusicalTimeSpanCalculator = Substitute.For<IMusicalTimeSpanCalculator>();
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        _neighborToneProcessor = new NeighborToneProcessor(_mockMusicalTimeSpanCalculator, _mockWeightedRandomBooleanGenerator, OrnamentationType.DelayedNeighborTone, compositionConfiguration);
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

        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth.Dotted(1));
        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Sixteenth);
        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(true);

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DelayedNeighborTone);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth.Dotted(1));
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
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

        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth.Dotted(1));
        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Sixteenth);
        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(false);

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DelayedNeighborTone);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth.Dotted(1));
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
    }
}
