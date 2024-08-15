using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class QuartetChordChoiceRepositoryTests
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

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Four))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Four, NoteMotion.Oblique, 0),
                    new(Voice.Four, NoteMotion.Ascending, 2),
                    new(Voice.Four, NoteMotion.Descending, 3)
                }
            );
    }

    [Test]
    public void WhenQuartetChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration();

        var quartetChordChoiceRepository = new QuartetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var noteChoiceCount = quartetChordChoiceRepository.Count;
        var noteChoice = quartetChordChoiceRepository.GetChordChoice(1);

        // assert
        noteChoiceCount.Should().Be(81);

        noteChoice.Should().BeEquivalentTo(
            new ChordChoice(
                [
                    new NoteChoice(Voice.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Two, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Three, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Four, NoteMotion.Ascending, 2)
                ]
            )
        );

        _mockNoteChoiceGenerator.Received(4).GenerateNoteChoices(Arg.Any<Voice>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToQuartetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        // act
        var act = () => _ = new QuartetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetChordChoiceId_ReturnsExpectedIndex()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration();

        var quartetChordChoiceRepository = new QuartetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var id = quartetChordChoiceRepository.GetChordChoiceId(
            new ChordChoice(
                [
                    new NoteChoice(Voice.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Two, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Three, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Four, NoteMotion.Ascending, 2)
                ]
            )
        );

        // assert
        id.Should().Be(1);
    }
}
