using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class PickupProcessorTests
{
    private IMusicalTimeSpanCalculator _mockMusicalTimeSpanCalculator = null!;

    private PickupProcessor _pickupProcessor = null!;

    [SetUp]
    public void SetUp()
    {
        _mockMusicalTimeSpanCalculator = Substitute.For<IMusicalTimeSpanCalculator>();

        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _pickupProcessor = new PickupProcessor(_mockMusicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.Pickup);
    }

    [Test]
    public void Process_applies_upper_pickup_note_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Quarter);
        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Quarter);

        // act
        _pickupProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pickup);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations.Should().HaveCount(1);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.F4);
    }

    [Test]
    public void Process_applies_lower_pickup_note_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.E4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Quarter);
        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Quarter);

        // act
        _pickupProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pickup);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations.Should().HaveCount(1);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
    }
}
