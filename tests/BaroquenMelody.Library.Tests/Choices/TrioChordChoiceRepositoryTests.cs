﻿using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices;

[TestFixture]
internal sealed class TrioChordChoiceRepositoryTests
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

        _mockNoteChoiceGenerator
            .GenerateNoteChoices(Arg.Is<Instrument>(instrument => instrument == Instrument.Three))
            .Returns(
                new HashSet<NoteChoice>
                {
                    new(Instrument.Three, NoteMotion.Oblique, 0),
                    new(Instrument.Three, NoteMotion.Ascending, 2),
                    new(Instrument.Three, NoteMotion.Descending, 3)
                }
            );
    }

    [Test]
    public void WhenTrioChordChoiceRepositoryIsConstructed_ItGeneratesNoteChoices()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(3);

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
                    new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0),
                    new NoteChoice(Instrument.Three, NoteMotion.Ascending, 2)
                ]
            )
        );

        _mockNoteChoiceGenerator.Received(3).GenerateNoteChoices(Arg.Any<Instrument>());
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToQuartetChordChoiceRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get();

        // act
        var act = () => _ = new TrioChordChoiceRepository(
            compositionConfiguration,
            _mockNoteChoiceGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }
}
