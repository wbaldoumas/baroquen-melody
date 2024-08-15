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
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Soprano))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Soprano, NoteMotion.Oblique, 0),
                    new(Voice.Soprano, NoteMotion.Ascending, 2),
                    new(Voice.Soprano, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Alto))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Alto, NoteMotion.Oblique, 0),
                    new(Voice.Alto, NoteMotion.Ascending, 2),
                    new(Voice.Alto, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Tenor))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Tenor, NoteMotion.Oblique, 0),
                    new(Voice.Tenor, NoteMotion.Ascending, 2),
                    new(Voice.Tenor, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Voice>(voice => voice == Voice.Bass))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Voice.Bass, NoteMotion.Oblique, 0),
                    new(Voice.Bass, NoteMotion.Ascending, 2),
                    new(Voice.Bass, NoteMotion.Descending, 3)
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
                    new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Bass, NoteMotion.Ascending, 2)
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
                    new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Bass, NoteMotion.Ascending, 2)
                ]
            )
        );

        // assert
        id.Should().Be(1);
    }
}
