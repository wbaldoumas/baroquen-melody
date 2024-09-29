using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Selection.Strategies;

[TestFixture]
internal sealed class CleanRandomNoteTests
{
    private IWeightedRandomBooleanGenerator _mockBooleanGenerator = null!;

    private CleanRandomNote _cleanRandomNote = null!;

    [SetUp]
    public void SetUp()
    {
        _mockBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        _cleanRandomNote = new CleanRandomNote(_mockBooleanGenerator);
    }

    [Test]
    public void Select_returns_primary_note_when_random_boolean_generator_returns_true()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Eighth);

        _mockBooleanGenerator.IsTrue().Returns(true);

        // act
        var result = _cleanRandomNote.Select(primaryNote, secondaryNote);

        // assert
        result.Should().Be(primaryNote);
    }

    [Test]
    public void Select_returns_secondary_note_when_random_boolean_generator_returns_false()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Eighth);

        _mockBooleanGenerator.IsTrue().Returns(false);

        // act
        var result = _cleanRandomNote.Select(primaryNote, secondaryNote);

        // assert
        result.Should().Be(secondaryNote);
    }
}
