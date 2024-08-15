﻿using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class DuetChordChoiceRepositoryTests
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
    }

    [Test]
    public void WhenDuetChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

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
                    new NoteChoice(Voice.One, NoteMotion.Ascending, 2),
                    new NoteChoice(Voice.Two, NoteMotion.Descending, 3)
                ]
            )
        );

        _mockNoteChoiceGenerator.Received(2).GenerateNoteChoices(Arg.Any<Voice>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToDuetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

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
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        var duetChordChoiceRepository = new DuetChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // act
        var id = duetChordChoiceRepository.GetChordChoiceId(
            new ChordChoice(
                [
                    new NoteChoice(Voice.One, NoteMotion.Ascending, 2),
                    new NoteChoice(Voice.Two, NoteMotion.Descending, 3)
                ]
            )
        );

        // assert
        id.Should().Be(5);
    }
}
