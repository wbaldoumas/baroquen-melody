using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices;

[TestFixture]
internal sealed class DuetChordChoiceRepositoryTests
{
    private INoteChoiceGenerator _mockNoteChoiceGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteChoiceGenerator = Substitute.For<INoteChoiceGenerator>();

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Instrument>(instrument => instrument == Instrument.One))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Instrument.One, NoteMotion.Oblique, 0),
                    new(Instrument.One, NoteMotion.Ascending, 2),
                    new(Instrument.One, NoteMotion.Descending, 3)
                }
            );

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Instrument>(instrument => instrument == Instrument.Two))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Instrument.Two, NoteMotion.Oblique, 0),
                    new(Instrument.Two, NoteMotion.Ascending, 2),
                    new(Instrument.Two, NoteMotion.Descending, 3)
                }
            );
    }

    [Test]
    public void WhenDuetChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        var duetChordChoiceRepository = new DuetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var noteChoiceCount = duetChordChoiceRepository.Count;
        var noteChoice = duetChordChoiceRepository.GetChordChoice(5);

        // assert
        noteChoiceCount.Should().Be(9);

        noteChoice.Should().BeEquivalentTo(
            new ChordChoice(
                [
                    new NoteChoice(Instrument.One, NoteMotion.Ascending, 2),
                    new NoteChoice(Instrument.Two, NoteMotion.Descending, 3)
                ]
            )
        );

        _mockNoteChoiceGenerator.Received(2).GenerateNoteChoices(Arg.Any<Instrument>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToDuetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(3);

        // act
        var act = () => _ = new DuetChordChoiceRepository(
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
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        var duetChordChoiceRepository = new DuetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var id = duetChordChoiceRepository.GetChordChoiceId(
            new ChordChoice(
                [
                    new NoteChoice(Instrument.One, NoteMotion.Ascending, 2),
                    new NoteChoice(Instrument.Two, NoteMotion.Descending, 3)
                ]
            )
        );

        // assert
        id.Should().Be(5);
    }
}
