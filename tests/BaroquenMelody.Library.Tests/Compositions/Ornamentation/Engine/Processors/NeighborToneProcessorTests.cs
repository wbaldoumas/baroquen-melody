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
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class NeighborToneProcessorTests
{
    private IMusicalTimeSpanCalculator _mockMusicalTimeSpanCalculator;

    private NeighborToneProcessor _neighborToneProcessor;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.G6),
                new(Voice.Alto, Notes.C2, Notes.G5),
                new(Voice.Tenor, Notes.C1, Notes.G4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _mockMusicalTimeSpanCalculator = Substitute.For<IMusicalTimeSpanCalculator>();

        _neighborToneProcessor = new NeighborToneProcessor(_mockMusicalTimeSpanCalculator, compositionConfiguration);
    }

    [Test]
    public void Process_applies_ornamentation_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)]))
        );

        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth.Dotted(1));
        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Sixteenth);

        // act
        _neighborToneProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.NeighborTone);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Eighth.Dotted(1));
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Match<Note>(note => note == Notes.D4 || note == Notes.B3);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }
}
