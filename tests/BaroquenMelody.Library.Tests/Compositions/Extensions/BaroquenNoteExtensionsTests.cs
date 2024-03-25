using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Extensions;

[TestFixture]
internal sealed class BaroquenNoteExtensionsTests
{
    [Test]
    public void ApplyNoteChoice_WhenMotionIsAscending_ThenNextNoteIsReturned()
    {
        // arrange
        var note = new BaroquenNote(Voice.Soprano, Note.Get(NoteName.A, 4));
        var scale = Scale.Parse("C Major");
        var noteChoice = new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 2);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice);

        // assert
        result.Raw.Should().Be(Note.Get(NoteName.C, 5));
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsDescending_ThenNextNoteIsReturned()
    {
        // arrange
        var note = new BaroquenNote(Voice.Soprano, Note.Get(NoteName.C, 5));
        var scale = Scale.Parse("C Major");
        var noteChoice = new NoteChoice(Voice.Soprano, NoteMotion.Descending, 2);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice);

        // assert
        result.Raw.Should().Be(Note.Get(NoteName.A, 4));
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsOblique_ThenNextNoteIsReturned()
    {
        // arrange
        var note = new BaroquenNote(Voice.Soprano, Note.Get(NoteName.C, 5));
        var scale = Scale.Parse("C Major");
        var noteChoice = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice);

        // assert
        result.Raw.Should().Be(Note.Get(NoteName.C, 5));
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsInvalid_ThenExceptionIsThrown()
    {
        // arrange
        var note = new BaroquenNote(Voice.Soprano, Note.Get(NoteName.C, 5));
        var scale = Scale.Parse("C Major");
        var noteChoice = new NoteChoice(Voice.Soprano, (NoteMotion)99, 0);

        // act
        var act = () => note.ApplyNoteChoice(scale, noteChoice);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
