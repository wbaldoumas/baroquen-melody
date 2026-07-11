using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Viewpoints;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Viewpoints;

[TestFixture]
internal sealed class MelodicLineTests
{
    [Test]
    public void MelodicLine_ExposesTheProposedNoteAndItsInstrument()
    {
        // arrange
        var nextNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        // act
        var melodicLine = new MelodicLine([], nextNote);

        // assert
        melodicLine.NextNote.Should().BeSameAs(nextNote);
        melodicLine.Instrument.Should().Be(Instrument.One);
    }

    [Test]
    public void PrecedingNote_IndexesChordsBackFromTheLastPrecedingChord()
    {
        // arrange
        var secondLastNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var lastNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);
        var otherVoiceNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);

        var melodicLine = new MelodicLine(
            [new BaroquenChord([secondLastNote, otherVoiceNote]), new BaroquenChord([lastNote, otherVoiceNote])],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert
        melodicLine.PrecedingNote(1).Should().BeSameAs(lastNote);
        melodicLine.PrecedingNote(2).Should().BeSameAs(secondLastNote);
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void PrecedingNote_ReturnsNullOutsideThePrecedingChords(int chordsBack)
    {
        // arrange
        var melodicLine = new MelodicLine(
            [
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]),
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])
            ],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act
        var precedingNote = melodicLine.PrecedingNote(chordsBack);

        // assert
        precedingNote.Should().BeNull();
    }

    [Test]
    public void PrecedingNote_ReturnsNullWhenTheVoiceIsAbsentFromThatChord()
    {
        // arrange: the voice is present in the last chord but absent from the second-last one
        var melodicLine = new MelodicLine(
            [
                new BaroquenChord([new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]),
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])
            ],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act
        var precedingNote = melodicLine.PrecedingNote(2);

        // assert
        precedingNote.Should().BeNull();
    }
}
