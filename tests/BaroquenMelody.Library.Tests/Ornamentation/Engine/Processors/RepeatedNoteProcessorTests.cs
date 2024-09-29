using BaroquenMelody.Infrastructure.Collections;
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
internal sealed class RepeatedNoteProcessorTests
{
    private IMusicalTimeSpanCalculator _mockMusicalTimeSpanCalculator;

    private RepeatedNoteProcessor _repeatedNoteProcessor;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(3);

        _mockMusicalTimeSpanCalculator = Substitute.For<IMusicalTimeSpanCalculator>();
        _repeatedNoteProcessor = new RepeatedNoteProcessor(_mockMusicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.RepeatedNote);
    }

    [Test]
    public void Process_applies_ornamentation_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth);
        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth);

        // act
        _repeatedNoteProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.RepeatedNote);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }
}
