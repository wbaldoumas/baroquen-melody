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
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class SustainedNoteProcessorTests
{
    private SustainedNoteProcessor _sustainedNoteProcessor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _sustainedNoteProcessor = new SustainedNoteProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_sustained_note_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _sustainedNoteProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];
        var nextNoteToAssert = ornamentationItem.NextBeat![Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Sustain);
        noteToAssert.Ornamentations.Should().BeEmpty();
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Whole);
        nextNoteToAssert.OrnamentationType.Should().Be(OrnamentationType.MidSustain);
        nextNoteToAssert.Ornamentations.Should().BeEmpty();
        nextNoteToAssert.MusicalTimeSpan.Should().Be(new MusicalTimeSpan());
    }
}
