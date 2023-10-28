using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Choices;

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
    public void WhenDuetChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80),
                new(Voice.Tenor, 35, 70),
                new(Voice.Bass, 25, 60)
            }
        );

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
                new List<NoteChoice>
                {
                    new(Voice.Soprano, NoteMotion.Oblique, 0),
                    new(Voice.Alto, NoteMotion.Oblique, 0),
                    new(Voice.Tenor, NoteMotion.Oblique, 0),
                    new(Voice.Bass, NoteMotion.Ascending, 2)
                }
            )
        );

        _mockNoteChoiceGenerator.Received(4).GenerateNoteChoices(Arg.Any<Voice>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToDuetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80)
            }
        );

        // act
        var act = () => _ = new QuartetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetChordChoiceIndex_ReturnsExpectedIndex()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80),
                new(Voice.Tenor, 35, 70),
                new(Voice.Bass, 25, 60)
            }
        );

        var quartetChordChoiceRepository = new QuartetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var index = quartetChordChoiceRepository.GetChordChoiceIndex(
            new ChordChoice(
                new List<NoteChoice>
                {
                    new(Voice.Soprano, NoteMotion.Oblique, 0),
                    new(Voice.Alto, NoteMotion.Oblique, 0),
                    new(Voice.Tenor, NoteMotion.Oblique, 0),
                    new(Voice.Bass, NoteMotion.Ascending, 2)
                }
            )
        );

        // assert
        index.Should().Be(1);
    }
}
