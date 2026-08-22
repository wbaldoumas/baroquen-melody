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

        _sustainedNoteProcessor = new SustainedNoteProcessor(compositionConfiguration);
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
        nextNoteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half);
    }

    [Test]
    public void Process_WithAFiguredNextNote_AbsorbsThePrincipalAndKeepsTheFigure()
    {
        // arrange - the backward direction of the tie contract: a bare repeat that PRECEDES a figured note
        // still ties into it (nothing in the sustain chain examines the NEXT note's ornamentation). The
        // processor extends by the figured note's ACTUAL, already-shortened principal span, so the timeline
        // stays exact: the hold resolves directly into the sounding sub-note while the figured principal
        // goes silent as MidSustain. The pad-breathing weight is tuned around this behavior, so it is
        // pinned here - a change to either direction of the tie must fail a test, not just a comment.
        var figuredNextNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter)
        {
            OrnamentationType = OrnamentationType.NeighborTone
        };

        figuredNextNote.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter));

        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([figuredNextNote]))
        );

        // act
        _sustainedNoteProcessor.Process(ornamentationItem);

        // assert
        var sustainedNote = ornamentationItem.CurrentBeat[Instrument.One];
        var absorbedNote = ornamentationItem.NextBeat![Instrument.One];

        sustainedNote.OrnamentationType.Should().Be(OrnamentationType.Sustain);
        sustainedNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half + MusicalTimeSpan.Quarter, "the sustain absorbs the figured note's actual, already-shortened principal span");
        absorbedNote.OrnamentationType.Should().Be(OrnamentationType.MidSustain, "the figured principal is silenced by the absorption");
        absorbedNote.Ornamentations.Should().NotBeEmpty("the gentle sub-note survives the absorption and still sounds");
    }
}
