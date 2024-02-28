using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Contexts.Extensions;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Contexts.Extensions;

[TestFixture]
internal sealed class NoteContextExtensionsTests
{
    [Test]
    [TestCase(60, 2, NoteMotion.Ascending, 64)]
    [TestCase(60, 2, NoteMotion.Descending, 57)]
    [TestCase(60, 0, NoteMotion.Oblique, 60)]
    public void ApplyNoteChoice_ShouldCalculateCorrectPitch(
        byte startPitch,
        byte scaleStepChange,
        NoteMotion noteMotion,
        byte expectedPitch)
    {
        // arrange
        var noteContext = new NoteContext(Voice.Soprano, startPitch.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var noteChoice = new NoteChoice(Voice.Soprano, noteMotion, scaleStepChange);

        // act
        var resultNote = noteContext.ApplyNoteChoice(noteChoice, Scale.Parse("C Major"));

        // assert
        resultNote.Note.Should().Be(expectedPitch.ToNote());
        resultNote.ArrivedFromNoteContext.Should().BeEquivalentTo(noteContext);
        resultNote.ArrivedFromNoteChoice.Should().BeEquivalentTo(noteChoice);
    }

    [Test]
    public void ApplyNoteChoice_WithUnsupportedMotion_ShouldThrowArgumentOutOfRangeException()
    {
        // arrange
        var noteContext = new NoteContext(Voice.Soprano, 60.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var noteChoice = new NoteChoice(Voice.Soprano, (NoteMotion)55, 5);

        // act
        var act = () => noteContext.ApplyNoteChoice(noteChoice, Scale.Parse("C Major"));

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ToChordContext_WithTwoNoteContexts_ShouldConvertToChordContext()
    {
        // arrange
        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.A, 3), NoteMotion.Oblique, NoteSpan.None);

        // act
        var resultChordContext = (noteContext1, noteContext2).ToChordContext();

        // assert
        resultChordContext.NoteContexts.Should().BeEquivalentTo(new[] { noteContext1, noteContext2 });
    }

    [Test]
    public void ToChordContext_WithThreeNoteContexts_ShouldConvertToChordContext()
    {
        // arrange
        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.A, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Tenor, Note.Get(NoteName.F, 3), NoteMotion.Oblique, NoteSpan.None);

        // act
        var resultChordContext = (noteContext1, noteContext2, noteContext3).ToChordContext();

        // assert
        resultChordContext.NoteContexts.Should().BeEquivalentTo(new[] { noteContext1, noteContext2, noteContext3 });
    }

    [Test]
    public void ToChordContext_WithFourNoteContexts_ShouldConvertToChordContext()
    {
        // arrange
        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.A, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Tenor, Note.Get(NoteName.F, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext4 = new NoteContext(Voice.Bass, Note.Get(NoteName.F, 2), NoteMotion.Oblique, NoteSpan.None);

        // act
        var resultChordContext = (noteContext1, noteContext2, noteContext3, noteContext4).ToChordContext();

        // assert
        resultChordContext.NoteContexts.Should().BeEquivalentTo(new[] { noteContext1, noteContext2, noteContext3, noteContext4 });
    }
}
