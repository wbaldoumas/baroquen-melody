using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Selection.Strategies;

[TestFixture]
internal sealed class CleanLowerNoteTests
{
    private CleanLowerNote _cleanLowerNote = null!;

    [SetUp]
    public void SetUp() => _cleanLowerNote = new CleanLowerNote();

    [Test]
    public void Select_returns_primary_note_when_primary_note_is_lower()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Eighth);

        // act
        var result = _cleanLowerNote.Select(primaryNote, secondaryNote);

        // assert
        result.Should().Be(primaryNote);
    }

    [Test]
    public void Select_returns_secondary_note_when_secondary_note_is_lower()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Eighth);

        // act
        var result = _cleanLowerNote.Select(primaryNote, secondaryNote);

        // assert
        result.Should().Be(secondaryNote);
    }

    [Test]
    public void Select_returns_null_when_notes_are_same_pitch()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Eighth);

        // act
        var result = _cleanLowerNote.Select(primaryNote, secondaryNote);

        // assert
        result.Should().BeNull();
    }
}
