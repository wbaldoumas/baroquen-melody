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
}
