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

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Processors;

[TestFixture]
internal sealed class SustainedNoteProcessorTests
{
    private SustainedNoteProcessor _sustainedNoteProcessor = null!;

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

        _sustainedNoteProcessor = new SustainedNoteProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_sustained_note_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)]))
        );

        // act
        _sustainedNoteProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];
        var nextNoteToAssert = ornamentationItem.NextBeat![Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Sustain);
        noteToAssert.Ornamentations.Should().BeEmpty();
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Half);
        nextNoteToAssert.OrnamentationType.Should().Be(OrnamentationType.Rest);
        nextNoteToAssert.Ornamentations.Should().BeEmpty();
        nextNoteToAssert.Duration.Should().Be(new MusicalTimeSpan());
    }
}
