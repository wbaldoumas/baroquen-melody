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
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class DecorateIntervalProcessorTests
{
    private DecorateIntervalProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _processor = new DecorateIntervalProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, 4);
    }

    [Test]
    public void Process_decorates_interval_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half)])),
            null
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.E5);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D5);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.E5);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }
}
