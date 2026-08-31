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

    [Test]
    public void PrecedingEventNote_CollapsesHeldDuplicatesIntoOneEvent()
    {
        // arrange: the last chord is a held-harmony duplicate of its predecessor, so it belongs to the same
        // harmonic event and the second event back is the chord before the hold.
        var secondLastEventNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var lastEventNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);
        var otherVoiceNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
        var heldChord = new BaroquenChord([lastEventNote, otherVoiceNote]);

        var melodicLine = new MelodicLine(
            [new BaroquenChord([secondLastEventNote, otherVoiceNote]), heldChord, new BaroquenChord(heldChord)],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert
        melodicLine.PrecedingEventNote(1).Should().BeSameAs(lastEventNote);
        melodicLine.PrecedingEventNote(2).Should().BeSameAs(secondLastEventNote);
        melodicLine.PrecedingEventNote(3).Should().BeNull();
    }

    [Test]
    public void PrecedingEventNote_CollapsesARunOfDuplicatesIntoOneEvent()
    {
        // arrange: a harmony sustained across several duplicated beats is still a single event.
        var secondLastEventNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var heldChord = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]);

        var melodicLine = new MelodicLine(
            [new BaroquenChord([secondLastEventNote]), heldChord, new BaroquenChord(heldChord), new BaroquenChord(heldChord)],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert
        melodicLine.PrecedingEventNote(2).Should().BeSameAs(secondLastEventNote);
        melodicLine.PrecedingEventNote(3).Should().BeNull();
    }

    [Test]
    public void PrecedingEventNote_ReadsDuplicatesOnRawPitchesDespiteDivergedOrnamentation()
    {
        // arrange: the ornamentation passes decorate a hold's paired beats independently (the ground's close
        // searches over decorated statements), so a duplicate must stay recognizable on raw pitches alone.
        var secondLastEventNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var heldChord = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]);
        var decoratedDuplicate = new BaroquenChord(heldChord);

        decoratedDuplicate[Instrument.One].MusicalTimeSpan = MusicalTimeSpan.Quarter;
        decoratedDuplicate[Instrument.One].Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter));

        var melodicLine = new MelodicLine(
            [new BaroquenChord([secondLastEventNote]), heldChord, decoratedDuplicate],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert
        melodicLine.PrecedingEventNote(2).Should().BeSameAs(secondLastEventNote);
    }

    [Test]
    public void PrecedingEventNote_DoesNotCollapseWhenAnotherVoiceMoves()
    {
        // arrange: this voice repeats its note, but the other voice moves, so the last chord is a fresh
        // harmony rather than a held duplicate and each chord stays its own event.
        var repeatedNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);

        var melodicLine = new MelodicLine(
            [
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]),
                new BaroquenChord([repeatedNote, new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]),
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)])
            ],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert: the second event back is the voice's repeated note, not the chord before it.
        melodicLine.PrecedingEventNote(2).Should().BeSameAs(repeatedNote);
    }

    [Test]
    public void PrecedingEventNote_DoesNotCollapseWhenTheChordsCarryDifferentVoices()
    {
        // arrange: the last chord repeats the shared voice's pitch but drops the other voice, so it is not a
        // duplicate of its predecessor.
        var repeatedNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);

        var melodicLine = new MelodicLine(
            [
                new BaroquenChord([repeatedNote, new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]),
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])
            ],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act + assert
        melodicLine.PrecedingEventNote(2).Should().BeSameAs(repeatedNote);
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void PrecedingEventNote_ReturnsNullOutsideThePrecedingEvents(int eventsBack)
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
        var precedingEventNote = melodicLine.PrecedingEventNote(eventsBack);

        // assert
        precedingEventNote.Should().BeNull();
    }

    [Test]
    public void PrecedingEventNote_ReturnsNullWhenTheVoiceIsAbsentFromThatEvent()
    {
        // arrange: the voice is present in the last event but absent from the second-last one
        var melodicLine = new MelodicLine(
            [
                new BaroquenChord([new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]),
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])
            ],
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        );

        // act
        var precedingEventNote = melodicLine.PrecedingEventNote(2);

        // assert
        precedingEventNote.Should().BeNull();
    }
}
