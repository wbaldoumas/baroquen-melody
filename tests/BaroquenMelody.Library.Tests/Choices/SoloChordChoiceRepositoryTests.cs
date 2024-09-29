using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices;

[TestFixture]
internal sealed class SoloChordChoiceRepositoryTests
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
    }

    [Test]
    public void WhenSoloChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(1);

        var soloChordChoiceRepository = new SoloChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var noteChoiceCount = soloChordChoiceRepository.Count;
        var noteChoice = soloChordChoiceRepository.GetChordChoice(2);

        // assert
        noteChoiceCount.Should().Be(3);
        noteChoice.Should().BeEquivalentTo(new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Descending, 3)]));
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToSoloChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(3);

        // act
        var act = () => _ = new SoloChordChoiceRepository(
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
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(1);

        var soloChordChoiceRepository = new SoloChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var id = soloChordChoiceRepository.GetChordChoiceId(
            new ChordChoice(
                [
                    new NoteChoice(Instrument.One, NoteMotion.Ascending, 2)
                ]
            )
        );

        // assert
        id.Should().Be(1);
    }
}
