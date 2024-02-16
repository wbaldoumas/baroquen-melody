﻿using BaroquenMelody.Library.Compositions;
using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
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
            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 5), NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Alto, Note.Get(NoteName.A, 4), NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Tenor, Note.Get(NoteName.F, 4), NoteMotion.Oblique, NoteSpan.Leap),
            new NoteContext(Voice.Bass, Note.Get(NoteName.F, 3), NoteMotion.Oblique, NoteSpan.Leap)
        });

        var chordChoice = new ChordChoice(new HashSet<NoteChoice>
        {
            new(Voice.Soprano, NoteMotion.Ascending, 2),
            new(Voice.Alto, NoteMotion.Descending, 1),
            new(Voice.Tenor, NoteMotion.Oblique, 0),
            new(Voice.Bass, NoteMotion.Ascending, 3)
        });

        var expectedNotes = new HashSet<VoicedNote>
        {
            new(Note.Get(NoteName.E, 5), Voice.Soprano, chordContext[Voice.Soprano], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Soprano)),
            new(Note.Get(NoteName.G, 4), Voice.Alto, chordContext[Voice.Alto], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Alto)),
            new(Note.Get(NoteName.F, 4), Voice.Tenor, chordContext[Voice.Tenor], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Tenor)),
            new(Note.Get(NoteName.B, 3), Voice.Bass, chordContext[Voice.Bass], chordChoice.NoteChoices.First(noteChoice => noteChoice.Voice == Voice.Bass))
        };

        // act
        var resultChord = chordContext.ApplyChordChoice(chordChoice, Scale.Parse("C Major"));

        // assert
        resultChord.VoicedNotes.Should().BeEquivalentTo(expectedNotes);
        resultChord.ChordContext.Should().Be(chordContext);
        resultChord.ChordChoice.Should().Be(chordChoice);
    }
}
