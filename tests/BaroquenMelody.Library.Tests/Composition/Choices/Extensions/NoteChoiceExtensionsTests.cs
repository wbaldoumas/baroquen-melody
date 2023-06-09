﻿using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Choices.Extensions;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Choices.Extensions;

[TestFixture]
internal sealed class NoteChoiceExtensionsTests
{
    [Test]
    public void ToChordChoice_TwoNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);

        var source = (note1, note2);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice(new HashSet<NoteChoice> { note1, note2 }));
    }

    [Test]
    public void ToChordChoice_ThreeNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);

        var source = (note1, note2, note3);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice(new HashSet<NoteChoice> { note1, note2, note3 }));
    }

    [Test]
    public void ToChordChoice_FourNotes_CreatesExpectedChordChoice()
    {
        // arrange
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5);

        var source = (note1, note2, note3, note4);

        // act
        var result = source.ToChordChoice();

        // assert
        result.Should().BeEquivalentTo(new ChordChoice(new HashSet<NoteChoice> { note1, note2, note3, note4 }));
    }
}
