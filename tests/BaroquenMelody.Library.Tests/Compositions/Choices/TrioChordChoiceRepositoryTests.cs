using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class TrioChordChoiceRepositoryTests
{
    private INoteChoiceGenerator _mockNoteChoiceGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteChoiceGenerator = Substitute.For<INoteChoiceGenerator>();

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.One))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.One, NoteMotion.Oblique, 0),
                    new(Voice.One, NoteMotion.Ascending, 2),
                    new(Voice.One, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Two))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Two, NoteMotion.Oblique, 0),
                    new(Voice.Two, NoteMotion.Ascending, 2),
                    new(Voice.Two, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Three))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Three, NoteMotion.Oblique, 0),
                    new(Voice.Three, NoteMotion.Ascending, 2),
                    new(Voice.Three, NoteMotion.Descending, 3)
                }
            );
    }

    [Test]
    public void WhenTrioChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        var trioChordChoiceRepository = new TrioChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var noteChoiceCount = trioChordChoiceRepository.Count;
        var noteChoice = trioChordChoiceRepository.GetChordChoice(1);

        // assert
        noteChoiceCount.Should().Be(27);

        noteChoice.Should().BeEquivalentTo(
            new ChordChoice(
                [
                    new NoteChoice(Voice.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Two, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Three, NoteMotion.Ascending, 2)
                ]
            )
        );

        _mockNoteChoiceGenerator.Received(3).GenerateNoteChoices(Arg.Any<Voice>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToQuartetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration();

        // act
        var act = () => _ = new TrioChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetChordChoiceId_ReturnsExpectedId()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        var trioChordChoiceRepository = new TrioChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var id = trioChordChoiceRepository.GetChordChoiceId(
            new ChordChoice(
                [
                    new NoteChoice(Voice.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Two, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Three, NoteMotion.Ascending, 2)
                ]
            )
        );

        // assert
        id.Should().Be(1);
    }
}
