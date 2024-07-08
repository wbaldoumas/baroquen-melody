using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
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
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.C5),
                new(Voice.Alto, Notes.C2, Notes.C4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _processor = new DecorateIntervalProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, 4);
    }

    [Test]
    public void Process_decorates_interval_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
            null
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.E5);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D5);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.E5);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }
}
