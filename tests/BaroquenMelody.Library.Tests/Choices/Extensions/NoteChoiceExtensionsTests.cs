﻿using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Choices.Extensions;
using BaroquenMelody.Library.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices.Extensions;

[TestFixture]
internal sealed class NoteChoiceExtensionsTests
{
    [Test]
    public void ToChordChoice_OneNote_CreatesExpectedChordChoice()
    {
        // arrange
        var note = new NoteChoice(Instrument.One, NoteMotion.Oblique, 0);

        // act
        var result = note.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice([note]));
    }

    [Test]
    public void ToChordChoice_TwoNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Instrument.One, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Instrument.Two, NoteMotion.Ascending, 2);

        var source = (note1, note2);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice([note1, note2]));
    }

    [Test]
    public void ToChordChoice_ThreeNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Instrument.One, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Instrument.Two, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Instrument.Three, NoteMotion.Descending, 3);

        var source = (note1, note2, note3);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice([note1, note2, note3]));
    }

    [Test]
    public void ToChordChoice_FourNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Instrument.One, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Instrument.Two, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Instrument.Three, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Instrument.Four, NoteMotion.Ascending, 5);

        var source = (note1, note2, note3, note4);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice([note1, note2, note3, note4]));
    }
}
