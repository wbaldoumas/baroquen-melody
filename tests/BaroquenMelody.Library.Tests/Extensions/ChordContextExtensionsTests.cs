﻿using BaroquenMelody.Library.Compositions;
using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Extensions;

[TestFixture]
internal sealed class ChordContextExtensionsTests
{
    [Test]
    public void ApplyChordChoice_ShouldApplyNoteChoicesToChord()
    {
        // arrange
        var chordContext = new ChordContext(new[]
        {
            new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Alto, 55, NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Tenor, 50, NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Bass, 45, NoteMotion.Oblique, NoteSpan.Leap)
        });

        var chordChoice = new ChordChoice(new HashSet<NoteChoice>
        {
            new(Voice.Soprano, NoteMotion.Ascending, 2),
            new(Voice.Alto, NoteMotion.Descending, 1),
            new(Voice.Tenor, NoteMotion.Oblique, 0),
            new(Voice.Bass, NoteMotion.Ascending, 3)
        });

        var expectedNotes = new HashSet<Note>
        {
            new(62, Voice.Soprano, chordContext[Voice.Soprano], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Soprano)),
            new(54, Voice.Alto, chordContext[Voice.Alto], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Alto)),
            new(50, Voice.Tenor, chordContext[Voice.Tenor], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Tenor)),
            new(48, Voice.Bass, chordContext[Voice.Bass], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Bass))
        };

        // act
        var resultChord = chordContext.ApplyChordChoice(chordChoice);

        // assert
        resultChord.Notes.Should().BeEquivalentTo(expectedNotes);
        resultChord.ChordContext.Should().Be(chordContext);
        resultChord.ChordChoice.Should().Be(chordChoice);
    }
}