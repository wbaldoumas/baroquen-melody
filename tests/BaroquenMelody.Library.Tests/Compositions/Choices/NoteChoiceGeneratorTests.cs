using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class NoteChoiceGeneratorTests
{
    private NoteChoiceGenerator _noteChoiceGenerator = null!;

    [SetUp]
    public void SetUp() => _noteChoiceGenerator = new NoteChoiceGenerator(1, 7);

    [Test]
    public void GenerateNoteChoices_GivenInstrument_ReturnsNoteChoices()
    {
        var noteChoices = _noteChoiceGenerator.GenerateNoteChoices(Instrument.One);

        noteChoices.Should().NotBeNull();
        noteChoices.Should().NotBeEmpty();

        noteChoices.Where(noteChoice => noteChoice.Motion == NoteMotion.Oblique)
            .Should()
            .HaveCount(1);

        noteChoices.Where(noteChoice => noteChoice.Motion == NoteMotion.Ascending)
            .Should()
            .HaveCount(7);

        noteChoices.Where(noteChoice => noteChoice.Motion == NoteMotion.Descending)
            .Should()
            .HaveCount(7);
    }
}
