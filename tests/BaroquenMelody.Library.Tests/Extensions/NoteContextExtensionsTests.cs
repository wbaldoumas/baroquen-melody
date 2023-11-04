using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using BaroquenMelody.Library.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Extensions;

[TestFixture]
internal sealed class NoteContextExtensionsTests
{
    [Test]
    [TestCase(60, 2, NoteMotion.Ascending, 62)]
    [TestCase(60, 2, NoteMotion.Descending, 58)]
    [TestCase(60, 0, NoteMotion.Oblique, 60)]
    public void ApplyNoteChoice_ShouldCalculateCorrectPitch(
        byte startPitch,
        byte pitchChange,
        NoteMotion noteMotion,
        byte expectedPitch)
    {
        // arrange
        var noteContext = new NoteContext(Voice.Soprano, startPitch, NoteMotion.Oblique, NoteSpan.None);
        var noteChoice = new NoteChoice(Voice.Soprano, noteMotion, pitchChange);

        // act
        var resultNote = noteContext.ApplyNoteChoice(noteChoice);

        // assert
        resultNote.Pitch.Should().Be(expectedPitch);
        resultNote.NoteContext.Should().BeEquivalentTo(noteContext);
        resultNote.NoteChoice.Should().BeEquivalentTo(noteChoice);
    }

    [Test]
    public void ApplyNoteChoice_WithUnsupportedMotion_ShouldThrowArgumentOutOfRangeException()
    {
        // arrange
        var noteContext = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var noteChoice = new NoteChoice(Voice.Soprano, (NoteMotion)55, 5);

        // act
        var act = () => noteContext.ApplyNoteChoice(noteChoice);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
