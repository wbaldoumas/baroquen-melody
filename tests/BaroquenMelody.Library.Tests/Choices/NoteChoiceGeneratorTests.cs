using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices;

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

    [Test]
    public void GenerateNoteChoices_ReturnsTheCanonicalOrder()
    {
        // The returned order defines the chord-choice index space shared by the choice repositories and the
        // forward-checking enumerator. It must be a pure function of the generator's inputs: a hash-ordered
        // set here once made seeded compositions differ between processes depending on earlier execution.
        var noteChoices = new NoteChoiceGenerator(1, 2).GenerateNoteChoices(Instrument.One);

        noteChoices.Should().Equal(
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 1),
            new NoteChoice(Instrument.One, NoteMotion.Descending, 1),
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 2),
            new NoteChoice(Instrument.One, NoteMotion.Descending, 2),
            new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)
        );
    }
}
