using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
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
        var note = new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half);
        var scale = BaroquenScale.Parse("C Major");
        var noteChoice = new NoteChoice(Instrument.One, NoteMotion.Ascending, 2);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice, MusicalTimeSpan.Half);

        // assert
        result.Raw.Should().Be(Notes.C5);
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsDescending_ThenNextNoteIsReturned()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half);
        var scale = BaroquenScale.Parse("C Major");
        var noteChoice = new NoteChoice(Instrument.One, NoteMotion.Descending, 2);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice, MusicalTimeSpan.Half);

        // assert
        result.Raw.Should().Be(Notes.A4);
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsOblique_ThenNextNoteIsReturned()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half);
        var scale = BaroquenScale.Parse("C Major");
        var noteChoice = new NoteChoice(Instrument.One, NoteMotion.Oblique, 0);

        // act
        var result = note.ApplyNoteChoice(scale, noteChoice, MusicalTimeSpan.Half);

        // assert
        result.Raw.Should().Be(Notes.C5);
    }

    [Test]
    public void ApplyNoteChoice_WhenMotionIsInvalid_ThenExceptionIsThrown()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half);
        var scale = BaroquenScale.Parse("C Major");
        var noteChoice = new NoteChoice(Instrument.One, (NoteMotion)99, 0);

        // act
        var act = () => note.ApplyNoteChoice(scale, noteChoice, MusicalTimeSpan.Half);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void IsDissonantWith_WhenNotDissonant_IsFalse()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half);
        var otherNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);

        // act
        var result = note.IsDissonantWith(otherNote);

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsDissonantWith_WhenDissonant_IsTrue()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half);
        var otherNote = new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half);

        // act
        var result = note.IsDissonantWith(otherNote);

        // assert
        result.Should().BeTrue();
    }
}
